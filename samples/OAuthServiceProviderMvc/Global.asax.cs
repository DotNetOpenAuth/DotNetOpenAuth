using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Text;
using DotNetOpenAuth.OAuth.Messages;
using System.Web.Mvc.Resources;

namespace oAuthMVC
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class Global : System.Web.HttpApplication
    {
        
        public static void RegisterRoutes(RouteCollection routes)
        {
            // We use this hook to inject our ResourceControllerActionInvoker which can smartly map HTTP verbs to Actions
            ResourceControllerFactory factory = new ResourceControllerFactory();
            ControllerBuilder.Current.SetControllerFactory(factory);

            // We use this hook to inject the ResourceModelBinder behavior which can de-serialize from xml/json formats 
            ModelBinders.Binders.DefaultBinder = new ResourceModelBinder();

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            );

        }

        protected void Application_Start()
        {            

            RegisterRoutes(RouteTable.Routes);
            Constants.WebRootUrl = new Uri(HttpContext.Current.Request.Url, "/");
            var tokenManager = new DatabaseTokenManager();
            Global.TokenManager = tokenManager;
        }

        /// <summary>
        /// An application memory cache of recent log messages.
        /// </summary>
        public static StringBuilder LogMessages = new StringBuilder();

        /// <summary>
        /// The logger for this sample to use.
        /// </summary>
        public static log4net.ILog Logger = log4net.LogManager.GetLogger("DotNetOpenAuth.ConsumerSample");


        /// <summary>
        /// Gets the transaction-protected database connection for the current request.
        /// </summary>
        public static DataClassesDataContext DataContext
        {
            get
            {
                DataClassesDataContext dataContext = dataContextSimple;
                if (dataContext == null)
                {
                    dataContext = new DataClassesDataContext();
                    dataContext.Connection.Open();
                    dataContext.Transaction = dataContext.Connection.BeginTransaction();
                    dataContextSimple = dataContext;
                }

                return dataContext;
            }
        }

        public static DatabaseTokenManager TokenManager { get; set; }

        public static User LoggedInUser
        {
            get { return Global.DataContext.Users.SingleOrDefault(user => user.OpenIDClaimedIdentifier == HttpContext.Current.User.Identity.Name); }
        }

        public static UserAuthorizationRequest PendingOAuthAuthorization
        {
            get { return HttpContext.Current.Session["authrequest"] as UserAuthorizationRequest; }
            set { HttpContext.Current.Session["authrequest"] = value; }
        }

        private static DataClassesDataContext dataContextSimple
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Items["DataContext"] as DataClassesDataContext;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items["DataContext"] = value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public static void AuthorizePendingRequestToken()
        {
            ITokenContainingMessage tokenMessage = PendingOAuthAuthorization;
            TokenManager.AuthorizeRequestToken(tokenMessage.Token, LoggedInUser);
            PendingOAuthAuthorization = null;
        }

        private static void CommitAndCloseDatabaseIfNecessary()
        {
            var dataContext = dataContextSimple;
            if (dataContext != null)
            {
                dataContext.SubmitChanges();
                dataContext.Transaction.Commit();
                dataContext.Connection.Close();
            }
        }

        private void Application_StartRequest(object sender, EventArgs e)
        {
            
        }

        private void Application_EndRequest(object sender, EventArgs e)
        {
            CommitAndCloseDatabaseIfNecessary();
        }
    }
}