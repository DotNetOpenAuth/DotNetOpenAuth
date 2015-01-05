namespace OAuthServiceProvider.Code {
	using System;
	using System.Linq;
	using System.ServiceModel;
	using System.Text;
	using System.Web;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// The web application global events and properties.
	/// </summary>
	public class Global : HttpApplication {
		/// <summary>
		/// An application memory cache of recent log messages.
		/// </summary>
		public static StringBuilder LogMessages = new StringBuilder();

		/// <summary>
		/// The logger for this sample to use.
		/// </summary>
		public static ILog Logger = LogProvider.GetLogger("DotNetOpenAuth.OAuthServiceProvider");

		private readonly object syncObject = new object();

		private volatile bool initialized;

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

		public static DatabaseNonceStore NonceStore { get; set; }

		public static User LoggedInUser {
			get { return Global.DataContext.Users.SingleOrDefault(user => user.OpenIDClaimedIdentifier == HttpContext.Current.User.Identity.Name); }
		}

		public static UserAuthorizationRequest PendingOAuthAuthorization {
			get { return HttpContext.Current.Session["authrequest"] as UserAuthorizationRequest; }
			set { HttpContext.Current.Session["authrequest"] = value; }
		}

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

		public static void AuthorizePendingRequestToken() {
			ITokenContainingMessage tokenMessage = PendingOAuthAuthorization;
			TokenManager.AuthorizeRequestToken(tokenMessage.Token, LoggedInUser);
			PendingOAuthAuthorization = null;
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
			Logger.Info("Sample starting...");
		}

		private void Application_End(object sender, EventArgs e) {
			Logger.Info("Sample shutting down...");
		}

		private void Application_Error(object sender, EventArgs e) {
			Logger.ErrorException("An unhandled exception occurred in ASP.NET processing: " + Server.GetLastError(), Server.GetLastError());

			// In the event of an unhandled exception, reverse any changes that were made to the database to avoid any partial database updates.
			var dataContext = dataContextSimple;
			if (dataContext != null) {
				dataContext.Transaction.Rollback();
				dataContext.Connection.Close();
				dataContext.Dispose();
				dataContextSimple = null;
			}
		}

		private void Application_BeginRequest(object sender, EventArgs e) {
			this.EnsureInitialized();
		}

		private void Application_EndRequest(object sender, EventArgs e) {
			CommitAndCloseDatabaseIfNecessary();
		}

		private void EnsureInitialized() {
			if (!this.initialized) {
				lock (this.syncObject) {
					if (!this.initialized) {
						string appPath = HttpContext.Current.Request.ApplicationPath;
						if (!appPath.EndsWith("/")) {
							appPath += "/";
						}

						Constants.WebRootUrl = new Uri(HttpContext.Current.Request.Url, appPath);
						Global.TokenManager = new DatabaseTokenManager();
						Global.NonceStore = new DatabaseNonceStore();
						this.initialized = true;
					}
				}
			}
		}
	}
}