using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

using Janrain.OpenId;
using Janrain.OpenId.Consumer;
using Janrain.OpenId.Store;

namespace Janrain.OpenId.Consumer.Asp
{
    public class ConsumerHttpModule : IHttpModule
    {
        public void Init( HttpApplication context )
        {   
            context.AcquireRequestState += new 
                    EventHandler(this.AuthenticateRequest);
        }

        private void AuthenticateRequest(Object sender, EventArgs e)
        {
            HttpContext Context = HttpContext.Current;
            HttpSessionState Session = Context.Session;
            HttpRequest Request = Context.Request;
            HttpResponse Response = Context.Response;
            Consumer consumer =  new Consumer(Session,
                MemoryStore.GetInstance());

            if (! Request.Url.AbsolutePath.EndsWith("/login.aspx"))
                return;

            if (Request.HttpMethod.ToLower() == "post")
            {
                string urlStr = Request.Form["openid_url"];
                if (urlStr != null)
                {
                    Uri userUri = UriUtil.NormalizeUri(urlStr);
                    try
                    {
                        // Initiate openid request
                        AuthRequest request = consumer.Begin(userUri);

                        // Build the trust root
                        UriBuilder builder = new UriBuilder(
                            Request.Url.AbsoluteUri);
                        builder.Query = null;
                        builder.Password = null;
                        builder.UserName = null;
                        builder.Fragment = null;
                        builder.Path = Request.ApplicationPath;
                        string trustRoot = builder.ToString();
                        
                        // Build the return_to URL
                        builder = new UriBuilder(Request.Url.AbsoluteUri);
                        NameValueCollection col = new NameValueCollection();
                        col["ReturnUrl"] = Request.QueryString["ReturnUrl"];
                        builder.Query = UriUtil.CreateQueryString(col);
                        Uri returnTo = new Uri(builder.ToString(), true);

                        Uri redirectUrl = request.CreateRedirect(
                            trustRoot, returnTo, AuthRequest.Mode.SETUP);
                        Response.Redirect(redirectUrl.AbsoluteUri);
                    }
                    catch (Exception fe)
                    {
                        Context.Items.Add("errmsg", fe.ToString());
                    }
                }
            }
            else if (Request.QueryString["openid.mode"] != null)
            {
                try
                {
                    ConsumerResponse resp = consumer.Complete(
                        Request.QueryString);
                    FormsAuthentication.RedirectFromLoginPage(
                        resp.IdentityUrl.AbsoluteUri, false);
                }
                catch (Exception fe)
                {
                    Context.Items.Add("errmsg", fe.ToString());
                }
            }
            else
            {
                Context.Items.Add("errmsg", "Please enter a valid url.");
            }
        }

        public void Dispose()
        {
        }
    }
}    
