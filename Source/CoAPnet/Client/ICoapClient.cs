﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoAPnet.Client
{
    public interface ICoapClient : IDisposable
    {
        Task ConnectAsync(CoapClientConnectOptions options, CancellationToken cancellationToken);

        ValueTask<CoapResponse> RequestAsync(CoapRequest request, CancellationToken cancellationToken);

        Task<CoapObserveResponse> ObserveAsync(CoapObserveOptions options, CancellationToken cancellationToken);

        Task StopObservationAsync(CoapObserveResponse observeResponse, CancellationToken cancellationToken);
    }
}
