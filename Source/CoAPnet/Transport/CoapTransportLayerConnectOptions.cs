using System.Net;

namespace CoAPnet.Transport
{
    public class CoapTransportLayerConnectOptions
    {
        public IPEndPoint EndPoint
        {
            get; set;
        }
        public IPEndPoint ClientEndPoint
        {
            get; set;
        }
    }
}
