using CoAPnet.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoAPnet.LowLevelClient
{
    public interface ILowLevelCoapClient : IDisposable
    {
        ValueTask SendAsync(CoapMessage message, CancellationToken cancellationToken);

        ValueTask<CoapMessage> ReceiveAsync(CancellationToken cancellationToken);
    }
}
