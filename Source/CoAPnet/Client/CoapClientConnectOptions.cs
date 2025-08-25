using CoAPnet.Protocol;
using CoAPnet.Transport;
using System;

namespace CoAPnet.Client
{
    public class CoapClientConnectOptions
    {
        public string Host
        {
            get; set;
        }

        public int Port { get; set; } = CoapDefaultPort.Unencrypted;
        public string ClientIP
        {
            get; set;
        } = "0.0.0.0";

        public int ClientPort { get; set; } = 0;

        public TimeSpan CommunicationTimeout { get; set; } = TimeSpan.FromSeconds(10);

        public Func<ICoapTransportLayer> TransportLayerFactory { get; set; } = () => new UdpCoapTransportLayer();
    }
}
