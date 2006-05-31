namespace Janrain.OpenId.Server

public abstract class Request:
    abstract Mode as string:
        get:
            pass

public abstract class AssociatedRequest(Request):
    internal assoc_handle as string

