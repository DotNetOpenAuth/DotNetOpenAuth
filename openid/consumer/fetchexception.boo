namespace Janrain.OpenId.Consumer

import  System

class FetchException(ApplicationException):
    public final response as FetchResponse

    def constructor(response as FetchResponse, message as string):
        super(message)
        self.response = response
