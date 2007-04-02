using System;
using System.Web;
using Janrain.OpenId;
using Janrain.OpenId.Server;

/// <summary>
/// Summary description for Util
/// </summary>
public class Util
{
    public Util()
    {
    }

    public static string ExtractUserName(Uri url)
    {        
        return url.Segments[url.Segments.Length - 1];
    }

    public static void GenerateHttpResponse(IEncodable response)
    {
        State.Session.Reset();
        Janrain.OpenId.Server.WebResponse webresponse = null;
        Janrain.OpenId.Server.Server server;
        try
        {
            server = new Janrain.OpenId.Server.Server(Janrain.OpenId.Store.MemoryStore.GetInstance());

            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("Preparing to send response");
            }
            #endregion
            
            webresponse = server.EncodeResponse(response);                        
        }
        catch (Janrain.OpenId.Server.EncodingException e)
        {
            string text = System.Text.Encoding.UTF8.GetString(
                e.Response.EncodeToKVForm());
            string error = @"
        <html><head><title>Error Processing Request</title></head><body>
        <p><pre>{0}</pre></p>
        <!--

        This is a large comment.  It exists to make this page larger.
        That is unfortunately necessary because of the 'smart'
        handling of pages returned with an error code in IE.

        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************

        --></body></html>";
            error = String.Format(error, HttpContext.Current.Server.HtmlEncode(text));
            HttpContext.Current.Response.StatusCode = 400;
            HttpContext.Current.Response.Write(error);
            HttpContext.Current.Response.Close();
        }

        if (((int)webresponse.Code) == 302)
        {
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace(String.Format("Send response as 302 browser redirect to: '{0}'", webresponse.Headers["Location"]));
            }
            #endregion
            
            HttpContext.Current.Response.Redirect(webresponse.Headers["Location"]);
            return;
        }
        HttpContext.Current.Response.StatusCode = (int)webresponse.Code;
        foreach (string key in webresponse.Headers)
            HttpContext.Current.Response.AddHeader(key, webresponse.Headers[key]);

        if (webresponse.Body != null)
        {
            
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("Send response as server side HTTP response");                
            }
            
            if (TraceUtil.Switch.TraceVerbose)
            {
                TraceUtil.ServerTrace("HTTP Response headers follows:");
                TraceUtil.ServerTrace(webresponse.Headers);
                TraceUtil.ServerTrace("HTTP Response follows:");
                TraceUtil.ServerTrace(System.Text.Encoding.UTF8.GetString(webresponse.Body));
            }            
            #endregion            
            
            HttpContext.Current.Response.Write(System.Text.Encoding.UTF8.GetString(webresponse.Body));
            
        }
        HttpContext.Current.Response.Flush();
        HttpContext.Current.Response.Close();
    }
    
}
