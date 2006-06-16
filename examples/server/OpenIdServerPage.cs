using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace Janrain.OpenId.Server.Asp
{
    public class BaseServerPage : Page
    {
        protected Janrain.OpenId.Store.MemoryStore store;
        protected Janrain.OpenId.Server.Server server;

        protected Uri ServerUri
        {
            get {
                UriBuilder builder = new UriBuilder(Request.Url);
                builder.Path = Response.ApplyAppPathModifier("~/server.aspx");
                builder.Query = null;
                builder.Fragment = null;
                return new Uri(builder.ToString(), true);
            }
        }

        public BaseServerPage()
        {
            this.store = Janrain.OpenId.Store.MemoryStore.GetInstance();
            this.server = new Janrain.OpenId.Server.Server(this.store);
        }
    
        protected void DisplayResponse(IEncodable response)
        {
            Session["last_request"] = null;
            Janrain.OpenId.Server.WebResponse webresponse = null;
            try
            {
                webresponse = this.server.EncodeResponse(response);
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
                error = String.Format(error, Server.HtmlEncode(text));
                Response.StatusCode = 400;
                Response.Write(error);
                Response.Close();
            }

            if (((int) webresponse.Code) == 302)
            {
                Response.Redirect(webresponse.Headers["Location"]);
                return;
            }
            Response.StatusCode = (int) webresponse.Code;
            foreach (string key in webresponse.Headers)
                Response.AddHeader(key, webresponse.Headers[key]);

            if (webresponse.Body != null)
                Response.Write(System.Text.Encoding.UTF8.GetString(
                    webresponse.Body));
            Response.Flush();
            Response.Close();
        }
    }

    public class OpenIdDecisionPage : BaseServerPage
    {
        protected Janrain.OpenId.Server.CheckIdRequest idrequest;
    
        protected void Page_Load(object src, EventArgs e)
        {
            idrequest = (Janrain.OpenId.Server.CheckIdRequest) Session["last_request"];
            String s = ServerHttpModule.ExtractUserName(
                idrequest.IdentityUrl, Request);
            if (s != User.Identity.Name)
            {
                FormsAuthentication.SignOut();
                Response.Redirect(Request.Url.AbsoluteUri);
            }
        }

        protected void Yes_Click(Object sender, EventArgs e) {
            Janrain.OpenId.Server.Response response = idrequest.Answer(
                true, ServerUri);
            DisplayResponse(response);
        }
        
        protected void No_Click(Object sender, EventArgs e) {
            Janrain.OpenId.Server.Response response = idrequest.Answer(
                false, ServerUri);
            DisplayResponse(response);
        }
    }

    public class OpenIdServerPage : BaseServerPage
    {
        protected void Page_Load (object src, System.EventArgs evt)
        {
            
            Janrain.OpenId.Server.Request request = null;
            try
            {
                if (Request.HttpMethod == "GET")
                    request = this.server.DecodeRequest(Request.QueryString);
                else
                    request = this.server.DecodeRequest(Request.Form);
            }
            catch (Janrain.OpenId.Server.ProtocolException e)
            {
                DisplayResponse(e);
                return;
            }
            if (request == null)
                return;

            Janrain.OpenId.Server.Response response = null;
            if (request is Janrain.OpenId.Server.CheckIdRequest)
            {
                Janrain.OpenId.Server.CheckIdRequest idrequest = (Janrain.OpenId.Server.CheckIdRequest) request;
                if (idrequest.Immediate)
                {
                    
                    String s = Janrain.OpenId.Server.Asp.ServerHttpModule.ExtractUserName(idrequest.IdentityUrl, Request);
                    bool allow = (s != User.Identity.Name);
                    response = idrequest.Answer(allow, ServerUri);
                }
                else
                {
                    Session["last_request"] = request;
                    Response.Redirect(HttpRuntime.AppDomainAppVirtualPath +
                                      "decide.aspx");
                }
            }
            else if (request is Janrain.OpenId.Server.CheckAuthRequest)
            {
                response = this.server.HandleRequest(
                    (Janrain.OpenId.Server.CheckAuthRequest) request);
            }
            else
            {
                response = this.server.HandleRequest(
                    (Janrain.OpenId.Server.AssociateRequest) request);
            }
            DisplayResponse(response);
        }

    }
}