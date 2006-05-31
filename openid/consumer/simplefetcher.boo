namespace Janrain.OpenId.Consumer

import System
import System.IO
import System.Net

class SimpleFetcher(Fetcher):
    override def Get(uri as Uri, maxRead as uint):
        request = WebRequest.Create(uri) as HttpWebRequest
        request.KeepAlive = false
        request.Method = 'GET'
        request.MaximumAutomaticRedirections = 10
        fresp = null
        try:
            response = request.GetResponse() as HttpWebResponse
            try:
                fresp = GetResponse(response, maxRead)
                if response.StatusCode == HttpStatusCode.OK:
                    return fresp
                message = response.StatusCode.ToString()
            ensure:
                response.Close()
        except e as WebException:
            response = e.Response as HttpWebResponse
            if response is not null:
                try:
                    fresp = GetResponse(response, maxRead)
                ensure:
                    response.Close()
            message = e.Message

        raise FetchException(fresp, message)

    override def Post(uri as Uri, body as (byte), maxRead as uint):
        request = WebRequest.Create(uri) as HttpWebRequest
        request.ReadWriteTimeout = 20
        request.KeepAlive = false
        request.Method = 'POST'
        request.MaximumAutomaticRedirections = 10
        request.ContentLength = body.Length
        request.ContentType = 'application/x-www-form-urlencoded'
        fresp = null
        try:
            outStream = request.GetRequestStream()
            outStream.Write(body, 0, body.Length)
            outStream.Close()
            response = request.GetResponse() as HttpWebResponse
            try:
                fresp = GetResponse(response, maxRead)
                if response.StatusCode == HttpStatusCode.OK:
                    return fresp
                message = response.StatusCode.ToString()
            ensure:
                response.Close()
        except e as WebException:
            response = e.Response as HttpWebResponse
            if response is not null:
                try:
                    fresp = GetResponse(response, maxRead)
                ensure:
                    response.Close()
            message = e.Message
        raise FetchException(fresp, message)


