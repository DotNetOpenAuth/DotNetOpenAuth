//-----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WebFormsRelyingParty {
	using System;
	using System.Data;
	using System.Data.SqlClient;
	using System.Linq;
	using System.ServiceModel;
	using System.Web;

	public class Global : System.Web.HttpApplication {
		private const string DataContextKey = "DataContext";

		private const string DataContextTransactionKey = "DataContextTransaction";

		/// <summary>
		/// The logger for this sample to use.
		/// </summary>
		private static log4net.ILog logger = log4net.LogManager.GetLogger("DotNetOpenAuth.ConsumerSample");

		public static log4net.ILog Logger {
			get { return logger; }
		}

		public static User LoggedInUser {
			get { return Global.DataContext.AuthenticationToken.Where(token => token.ClaimedIdentifier == HttpContext.Current.User.Identity.Name).Select(token => token.User).FirstOrDefault(); }
		}

		public static string ApplicationPath {
			get {
				string path = HttpContext.Current.Request.ApplicationPath;
				if (!path.EndsWith("/")) {
					path += "/";
				}

				return path;
			}
		}

		/// <summary>
		/// Gets the transaction-protected database connection for the current request.
		/// </summary>
		public static DatabaseEntities DataContext {
			get {
				DatabaseEntities dataContext = DataContextSimple;
				if (dataContext == null) {
					dataContext = new DatabaseEntities();
					try {
						dataContext.Connection.Open();
					} catch (EntityException entityEx) {
						var sqlEx = entityEx.InnerException as SqlException;
						if (sqlEx != null) {
							if (sqlEx.Class == 14 && sqlEx.Number == 15350) {
								// Most likely the database schema hasn't been created yet.
								HttpContext.Current.Response.Redirect("~/Setup.aspx");
							}
						}

						throw;
					}

					DataContextTransactionSimple = dataContext.Connection.BeginTransaction();
					DataContextSimple = dataContext;
				}

				return dataContext;
			}
		}

		private static DatabaseEntities DataContextSimple {
			get {
				if (HttpContext.Current != null) {
					return HttpContext.Current.Items[DataContextKey] as DatabaseEntities;
				} else if (OperationContext.Current != null) {
					object data;
					if (OperationContext.Current.IncomingMessageProperties.TryGetValue(DataContextKey, out data)) {
						return data as DatabaseEntities;
					} else {
						return null;
					}
				} else {
					throw new InvalidOperationException();
				}
			}

			set {
				if (HttpContext.Current != null) {
					HttpContext.Current.Items[DataContextKey] = value;
				} else if (OperationContext.Current != null) {
					OperationContext.Current.IncomingMessageProperties[DataContextKey] = value;
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		private static IDbTransaction DataContextTransactionSimple {
			get {
				if (HttpContext.Current != null) {
					return HttpContext.Current.Items[DataContextTransactionKey] as IDbTransaction;
				} else if (OperationContext.Current != null) {
					object data;
					if (OperationContext.Current.IncomingMessageProperties.TryGetValue(DataContextTransactionKey, out data)) {
						return data as IDbTransaction;
					} else {
						return null;
					}
				} else {
					throw new InvalidOperationException();
				}
			}

			set {
				if (HttpContext.Current != null) {
					HttpContext.Current.Items[DataContextTransactionKey] = value;
				} else if (OperationContext.Current != null) {
					OperationContext.Current.IncomingMessageProperties[DataContextTransactionKey] = value;
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		protected void Application_Start(object sender, EventArgs e) {
			log4net.Config.XmlConfigurator.Configure();
			Logger.Info("Web application starting...");
		}

		protected void Session_Start(object sender, EventArgs e) {
		}

		protected void Application_BeginRequest(object sender, EventArgs e) {
		}

		protected void Application_EndRequest(object sender, EventArgs e) {
			CommitAndCloseDatabaseIfNecessary();
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e) {
		}

		protected void Application_Error(object sender, EventArgs e) {
			Logger.Error("An unhandled exception occurred in ASP.NET processing: " + Server.GetLastError(), Server.GetLastError());
			if (DataContextTransactionSimple != null) {
				DataContextTransactionSimple.Rollback();
				DataContextTransactionSimple.Dispose();
			}
		}

		protected void Session_End(object sender, EventArgs e) {
		}

		protected void Application_End(object sender, EventArgs e) {
			Logger.Info("Web application shutting down...");

			// this would be automatic, but in partial trust scenarios it is not.
			log4net.LogManager.Shutdown();
		}

		private static void CommitAndCloseDatabaseIfNecessary() {
			var dataContext = DataContextSimple;
			if (dataContext != null) {
				dataContext.SaveChanges();
				if (DataContextTransactionSimple != null) {
					DataContextTransactionSimple.Commit();
					DataContextTransactionSimple.Dispose();
				}

				dataContext.Dispose();
				DataContextSimple = null;
			}
		}
	}
}