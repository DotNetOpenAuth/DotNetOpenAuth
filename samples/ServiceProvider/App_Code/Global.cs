using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.ServiceModel;
using DotNetOAuth.Messages;

/// <summary>
/// Summary description for Global
/// </summary>
public class Global : HttpApplication {
	/// <summary>
	/// An application memory cache of recent log messages.
	/// </summary>
	public static StringBuilder LogMessages = new StringBuilder();

	/// <summary>
	/// The logger for this sample to use.
	/// </summary>
	public static log4net.ILog Logger = log4net.LogManager.GetLogger("DotNetOAuth.ConsumerSample");

	private static DataClassesDataContext dataContextSimple {
		get {
			if (HttpContext.Current != null) {
				return HttpContext.Current.Items["DataContext"] as DataClassesDataContext;
			} else if (OperationContext.Current != null) {
				object data;
				if (OperationContext.Current.IncomingMessageProperties.TryGetValue("DataContext", out data)) {
					return data as DataClassesDataContext;
				} else {
					return null;
				}
			} else {
				throw new InvalidOperationException();
			}
		}
		set {
			if (HttpContext.Current != null) {
				HttpContext.Current.Items["DataContext"] = value;
			} else if (OperationContext.Current != null) {
				OperationContext.Current.IncomingMessageProperties["DataContext"] = value;
			} else {
				throw new InvalidOperationException();
			}
		}
	}

	/// <summary>
	/// Gets the transaction-protected database connection for the current request.
	/// </summary>
	public static DataClassesDataContext DataContext {
		get {
			DataClassesDataContext dataContext = dataContextSimple;
			if (dataContext == null) {
				dataContext = new DataClassesDataContext();
				dataContext.Connection.Open();
				dataContext.Transaction = dataContext.Connection.BeginTransaction();
				dataContextSimple = dataContext;
			}

			return dataContext;
		}
	}

	public static DatabaseTokenManager TokenManager { get; set; }

	public static User LoggedInUser {
		get { return Global.DataContext.Users.SingleOrDefault(user => user.OpenIDClaimedIdentifier == HttpContext.Current.User.Identity.Name); }
	}

	public static DirectUserToServiceProviderMessage PendingOAuthAuthorization {
		get { return HttpContext.Current.Session["authrequest"] as DirectUserToServiceProviderMessage; }
		set { HttpContext.Current.Session["authrequest"] = value; }
	}

	private static void CommitAndCloseDatabaseIfNecessary() {
		var dataContext = dataContextSimple;
		if (dataContext != null) {
			dataContext.SubmitChanges();
			dataContext.Transaction.Commit();
			dataContext.Connection.Close();
		}
	}

	private void Application_Start(object sender, EventArgs e) {
		log4net.Config.XmlConfigurator.Configure();
		Logger.Info("Sample starting...");
		Constants.WebRootUrl = new Uri(HttpContext.Current.Request.Url, "/");
		var tokenManager = new DatabaseTokenManager();
		Global.TokenManager = tokenManager;
	}

	private void Application_End(object sender, EventArgs e) {
		Logger.Info("Sample shutting down...");
		// this would be automatic, but in partial trust scenarios it is not.
		log4net.LogManager.Shutdown();
	}

	private void Application_Error(object sender, EventArgs e) {
		Logger.Error("An unhandled exception occurred in ASP.NET processing: " + Server.GetLastError(), Server.GetLastError());
	}

	private void Application_EndRequest(object sender, EventArgs e) {
		CommitAndCloseDatabaseIfNecessary();
	}

	public static void AuthorizePendingRequestToken() {
		TokenManager.AuthorizeRequestToken(PendingOAuthAuthorization.RequestToken, LoggedInUser);
		PendingOAuthAuthorization = null;
	}
}
