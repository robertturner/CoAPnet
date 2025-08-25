namespace CoAPnet.Client
{
    public sealed class CoapResponse
    {
        public CoapStatusCode StatusCode
        {
            get; set;
        }

        public CoapResponseOptions Options
        {
            get; set;
        }

        public byte[] Payload
        {
            get; set;
        }
    }
}

