﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoAPnet.Transport
{
    public sealed class TlsCoapTransportLayer : ICoapTransportLayer
    {
        public Task ConnectAsync(CoapTransportLayerConnectOptions connectOptions, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
