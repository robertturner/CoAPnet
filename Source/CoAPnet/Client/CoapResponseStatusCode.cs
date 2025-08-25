namespace CoAPnet.Client
{
    public enum CoapResponseStatusCode
    {
        Empty = 0,

        Created = 201,
        Deleted = 202,
        Valid = 203,
        Changed = 204,
        Content = 205,

        BadRequest = 400,
        Unauthorized = 401,
        BadOption = 402,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        NotAcceptable = 406,
        PreconditionFailed = 412,
        RequestEntityTooLarge = 413,
        UnsupportedContentFormat = 415,

        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
        ProxyingNotSupported = 505
    }

    public enum CoapStatusCode
    {
        Empty = 0,

        Created = (2 << 5) + 1,
        Deleted = (2 << 5) + 2,
        Valid = (2 << 5) + 3,
        Changed = (2 << 5) + 4,
        Content = (2 << 5) + 5,
        Continue = (2 << 5) + 31,

        BadRequest = (4 << 5) + 0,
        Unauthorized = (4 << 5) + 1,
        BadOption = (4 << 5) + 2,
        Forbidden = (4 << 5) + 3,
        NotFound = (4 << 5) + 4,
        MethodNotAllowed = (4 << 5) + 5,
        NotAcceptable = (4 << 5) + 6,
        PreconditionFailed = (4 << 5) + 12,
        RequestEntityTooLarge = (4 << 5) + 13,
        UnsupportedContentFormat = (4 << 5) + 15,

        InternalServerError = (5 << 5) + 0,
        NotImplemented = (5 << 5) + 1,
        BadGateway = (5 << 5) + 2,
        ServiceUnavailable = (5 << 5) + 3,
        GatewayTimeout = (5 << 5) + 4,
        ProxyingNotSupported = (5 << 5) + 5
    }

}

