namespace Janrain.OpenId.Consumer

import System
import System.Net
import System.IO

abstract class Fetcher:
    public static MAX_BYTES as uint = (1024 * 1024)

    # 1MB
    protected static def ReadData(resp as HttpWebResponse, max_bytes as uint,
				  ref buffer as (byte)):
        ms as MemoryStream = null
        stream = resp.GetResponseStream()
        length = cast(int, resp.ContentLength)
        nolength = (length == (-1))
        size = (8192 if nolength else length)
        if nolength:
            ms = MemoryStream()

        size = Math.Min(size, cast(int, max_bytes))
        nread = 0
        offset = 0
        buffer = array(byte, size)
        while (nread = stream.Read(buffer, offset, size)) != 0:
            if nolength:
                ms.Write(buffer, 0, nread)
            else:
                size -= nread
                offset += nread

        if nolength:
            buffer = ms.ToArray()
            offset = buffer.Length

        return offset

    protected static def GetResponse(resp as HttpWebResponse, maxRead as uint):
        data as (byte) = null
        length as int = ReadData(resp, maxRead, data)
        return FetchResponse(resp.StatusCode, resp.ResponseUri,
			     resp.CharacterSet, data, length)

    abstract def Get(uri as Uri, maxRead as uint) as FetchResponse:
        pass

    virtual def Get(uri as Uri) as FetchResponse:
        return Get(uri, MAX_BYTES)

    abstract def Post(uri as Uri, body as (byte),
		      maxRead as uint) as FetchResponse:
        pass

    virtual def Post(uri as Uri, body as (byte)) as FetchResponse:
        return Post(uri, body, MAX_BYTES)

