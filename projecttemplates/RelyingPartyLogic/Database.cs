//-----------------------------------------------------------------------
// <copyright file="Database.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.EntityClient;
	using System.Data.SqlClient;
	using System.Linq;
	using System.ServiceModel;
	using System.Text;
	using System.Web;

	public class Database : IHttpModule, IDisposable {
		private const string DataContextKey = "DataContext";

		private const string DataContextTransactionKey = "DataContextTransaction";

		/// <summary>
		/// Initializes a new instance of the <see cref="Database"/> class.
		/// </summary>
		public Database() {
		}

		public static User LoggedInUser {
			get { return DataContext.AuthenticationTokens.Where(token => token.ClaimedIdentifier == HttpContext.Current.User.Identity.Name).Select(token => token.User).FirstOrDefault(); }
		}

		/// <summary>
		/// Gets the transaction-protected database connection for the current request.
		/// </summary>
		public static DatabaseEntities DataContext {
			get {
				DatabaseEntities dataContext = DataContextSimple;
				if (dataContext == null) {
					dataContext = new DatabaseEntities();
					dataContext.Connection.Open();
					DataContextTransaction = (EntityTransaction)dataContext.Connection.BeginTransaction();
					DataContextSimple = dataContext;
				}

				return dataContext;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the data context is already initialized.
		/// </summary>
		internal static bool IsDataContextInitialized {
			get { return DataContextSimple != null; }
		}

		internal static EntityTransaction DataContextTransaction {
			get {
				if (HttpContext.Current != null) {
					return HttpContext.Current.Items[DataContextTransactionKey] as EntityTransaction;
				} else if (OperationContext.Current != null) {
					object data;
					if (OperationContext.Current.IncomingMessageProperties.TryGetValue(DataContextTransactionKey, out data)) {
						return data as EntityTransaction;
					} else {
						return null;
					}
				} else {
					throw new InvalidOperationException();
				}
			}

			private set {
				if (HttpContext.Current != null) {
					HttpContext.Current.Items[DataContextTransactionKey] = value;
				} else if (OperationContext.Current != null) {
					OperationContext.Current.IncomingMessageProperties[DataContextTransactionKey] = value;
				} else {
					throw new InvalidOperationException();
				}
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

		public void Dispose() {
		}

		void IHttpModule.Init(HttpApplication context) {
			context.EndRequest += this.Application_EndRequest;
			context.Error += this.Application_Error;
		}

		protected void Application_EndRequest(object sender, EventArgs e) {
			CommitAndCloseDatabaseIfNecessary();
		}

		protected void Application_Error(object sender, EventArgs e) {
			if (DataContextTransaction != null) {
				DataContextTransaction.Rollback();
				DataContextTransaction.Dispose();
				DataContextTransaction = null;
			}
		}

		private static void CommitAndCloseDatabaseIfNecessary() {
			var dataContext = DataContextSimple;
			if (dataContext != null) {
				dataContext.SaveChanges();
				if (DataContextTransaction != null) {
					DataContextTransaction.Commit();
					DataContextTransaction.Dispose();
				}

				dataContext.Dispose();
				DataContextSimple = null;
			}
		}
	}
}
