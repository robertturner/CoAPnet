﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoAPnet.Internal;
using CoAPnet.Logging;
using CoAPnet.LowLevelClient;
using CoAPnet.MessageDispatcher;
using CoAPnet.Protocol;
using CoAPnet.Protocol.Observe;
using CoAPnet.Protocol.Options;

namespace CoAPnet.Client
{
    public sealed class CoapClient : ICoapClient
    {
        readonly CoapNetLogger _logger;
        readonly LowLevelCoapClient _lowLevelClient;
        readonly CoapMessageDispatcher _messageDispatcher = new CoapMessageDispatcher();
        readonly CoapMessageIdProvider _messageIdProvider = new CoapMessageIdProvider();
        readonly CoapMessageTokenProvider _messageTokenProvider = new CoapMessageTokenProvider();
        readonly CoapMessageToResponseConverter _messageToResponseConverter = new CoapMessageToResponseConverter();
        readonly CoapClientObservationManager _observationManager;
        readonly CoapRequestToMessageConverter _requestToMessageConverter = new CoapRequestToMessageConverter();
        
        CancellationTokenSource _cancellationToken;

        CoapClientConnectOptions _connectOptions;

        public CoapClient(CoapNetLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _lowLevelClient = new LowLevelCoapClient(_logger);
            _observationManager =
                new CoapClientObservationManager(_messageToResponseConverter, _lowLevelClient, _logger);
        }

        public async Task ConnectAsync(CoapClientConnectOptions options, CancellationToken cancellationToken)
        {
            _connectOptions = options ?? throw new ArgumentNullException(nameof(options));

            await _lowLevelClient.ConnectAsync(options, cancellationToken).ConfigureAwait(false);

            _cancellationToken = new CancellationTokenSource();
            ParallelTask.StartLongRunning(() =>
            {
                //Debug.WriteLine($"Long Running Start ID: {Environment.CurrentManagedThreadId}");
                var ret = ReceiveMessages(_cancellationToken.Token);
                //Debug.WriteLine($"Long Running End ID: {Environment.CurrentManagedThreadId}");
                return ret;
            }, _cancellationToken.Token);
        }

        public async ValueTask<CoapResponse> RequestAsync(CoapRequest request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestMessage = _requestToMessageConverter.Convert(request);

            var responseMessage = await RequestAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            var payload = responseMessage.Payload;
            if (CoapClientBlockTransferReceiver.IsBlockTransfer(responseMessage))
            {
                payload = await new CoapClientBlockTransferReceiver(requestMessage, responseMessage, this, _logger)
                    .ReceiveFullPayload(cancellationToken).ConfigureAwait(false);
            }

            return _messageToResponseConverter.Convert(responseMessage, payload);
        }

        public async Task<CoapObserveResponse> ObserveAsync(CoapObserveOptions options,
            CancellationToken cancellationToken)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var request = new CoapRequest
            {
                Method = CoapRequestMethod.Get,
                Options = options.Request.Options
            };

            var token = _messageTokenProvider.Next();

            var requestMessage = _requestToMessageConverter.Convert(request);
            requestMessage.Token = token.Value;
            requestMessage.Options.Add(new CoapMessageOptionFactory().CreateObserve(CoapObserveOptionValue.Register));

            var responseMessage = await RequestAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            var payload = responseMessage.Payload;
            if (CoapClientBlockTransferReceiver.IsBlockTransfer(responseMessage))
            {
                payload = await new CoapClientBlockTransferReceiver(requestMessage, responseMessage, this, _logger)
                    .ReceiveFullPayload(cancellationToken).ConfigureAwait(false);
            }

            _observationManager.Register(token, options.ResponseHandler);

            var response = _messageToResponseConverter.Convert(responseMessage, payload);
            return new CoapObserveResponse(response, this)
            {
                Token = token,
                Request = request
            };
        }

        public async Task StopObservationAsync(CoapObserveResponse observeResponse, CancellationToken cancellationToken)
        {
            if (observeResponse is null)
            {
                throw new ArgumentNullException(nameof(observeResponse));
            }

            var requestMessage = _requestToMessageConverter.Convert(observeResponse.Request);
            requestMessage.Token = observeResponse.Token.Value;

            requestMessage.Options.RemoveAll(o => o.Number == CoapMessageOptionNumber.Observe);
            requestMessage.Options.Add(new CoapMessageOptionFactory().CreateObserve(CoapObserveOptionValue.Deregister));

            var responseMessage = await RequestAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            _observationManager.Deregister(observeResponse.Token);
        }

        public void Dispose()
        {
            try
            {
                _cancellationToken?.Cancel(false);
            }
            finally
            {
                _cancellationToken?.Dispose();
                _lowLevelClient?.Dispose();
            }
        }

        internal async ValueTask<CoapMessage> RequestAsync(CoapMessage requestMessage, CancellationToken cancellationToken)
        {
            if (requestMessage is null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            requestMessage.Id = _messageIdProvider.Next();

            var responseAwaiter = _messageDispatcher.AddAwaiter(requestMessage.Id);
            
            try
            {
                await _lowLevelClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

                var responseMessage = await responseAwaiter.WaitOneAsync(_connectOptions.CommunicationTimeout)
                //var responseMessage = await responseAwaiter.WaitOneAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (responseMessage.Code.Equals(CoapMessageCodes.Empty))
                {
                    // TODO: Support message which are sent later (no piggybacking).
                }

                return responseMessage;
            }
            finally
            {
                _messageDispatcher.RemoveAwaiter(requestMessage.Id);
            }
        }

        async Task ReceiveMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var message = await _lowLevelClient.ReceiveAsync(cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (message == null)
                    {
                        continue;
                    }

                    if (!_messageDispatcher.TryHandleReceivedMessage(message))
                    {
                        if (!await _observationManager.TryHandleReceivedMessage(message).ConfigureAwait(false))
                        {
                            _logger.Trace(nameof(CoapClient), "Received an unexpected message ({0}).", message.Id);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    if (!_cancellationToken.IsCancellationRequested)
                    {
                        _logger.Error(nameof(CoapClient), exception, "Error while receiving messages.");
                    }
                    else
                    {
                        _logger.Information(nameof(CoapClient), "Stopped receiving messages due to cancellation.");
                    }
                    
                    _messageDispatcher.Dispatch(exception);
                }
            }
        }
    }
}