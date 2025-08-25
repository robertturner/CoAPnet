using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoAPnet.Transport
{
    public interface ICoapTransportLayer : IDisposable
    {
        Task ConnectAsync(CoapTransportLayerConnectOptions connectOptions, CancellationToken cancellationToken);

        ValueTask SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);

        ValueTask<int> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
    }
}
