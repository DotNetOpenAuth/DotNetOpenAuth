using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DotNetOpenId.Server;

/// <summary>
/// Summary description for State
/// </summary>
public class State
{
    public State()
    {        
    }
    
    public static SessionState Session
    {
        get
        {
            if (HttpContext.Current.Session["SessionState"] == null)  { HttpContext.Current.Session["SessionState"] = new SessionState(); }
            return HttpContext.Current.Session["SessionState"] as SessionState;
        }
    }

    public static  Uri ServerUri
    {
        get
        {
            UriBuilder builder = new UriBuilder(HttpContext.Current.Request.Url);
            builder.Path = HttpContext.Current.Response.ApplyAppPathModifier("~/server.aspx");
            builder.Query = null;
            builder.Fragment = null;
            return new Uri(builder.ToString(), true);
        }
    }

    [Serializable()]
    public  class SessionState
    {
        private CheckIdRequest lastRequest;

        public CheckIdRequest LastRequest
        {
            get { return lastRequest; }
            set { lastRequest = value; }
        }
        
        public void CheckExpectedStateIsAvailable()
        {
            if (LastRequest == null)
            {
                throw new ApplicationException("The CheckIdRequest has not been set. This usually means that Http Session is not available and the OpenID request needs to be restarted.");
            }
        }
        
        public void Reset()
        {
            lastRequest = null;
        }
    }


}
