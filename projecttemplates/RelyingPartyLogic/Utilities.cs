//-----------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using System.Data.EntityClient;
	using System.Data.Objects;
	using System.Data.SqlClient;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.OpenId;
	using Microsoft.SqlServer.Management.Common;
	using Microsoft.SqlServer.Management.Smo;

	public static class Utilities {
		internal const string DefaultNamespace = "RelyingPartyLogic";

		/// <summary>
		/// Gets the full URI of the web application root.  Guaranteed to end in a slash.
		/// </summary>
		public static Uri ApplicationRoot {
			get {
				string appRoot = HttpContext.Current.Request.ApplicationPath;
				if (!appRoot.EndsWith("/", StringComparison.Ordinal)) {
					appRoot += "/";
				}

				return new Uri(HttpContext.Current.Request.Url, appRoot);
			}
		}

		public static void CreateDatabase(Identifier claimedId, string friendlyId, string databaseName) {
			const string SqlFormat = @"
{0}
GO
EXEC [dbo].[AddUser] 'admin', 'admin', '{1}', '{2}'
GO
";
			var removeSnippets = new string[] { @"
IF IS_SRVROLEMEMBER(N'sysadmin') = 1
    BEGIN
        IF EXISTS (SELECT 1
                   FROM   [master].[dbo].[sysdatabases]
                   WHERE  [name] = N'$(DatabaseName)')
            BEGIN
                EXECUTE sp_executesql N'ALTER DATABASE [$(DatabaseName)]
    SET HONOR_BROKER_PRIORITY OFF 
    WITH ROLLBACK IMMEDIATE';
            END
    END
ELSE
    BEGIN
        PRINT N'The database settings cannot be modified. You must be a SysAdmin to apply these settings.';
    END


GO" };
			string databasePath = HttpContext.Current.Server.MapPath("~/App_Data/" + databaseName + ".mdf");
			StringBuilder schemaSqlBuilder = new StringBuilder();
			using (var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(DefaultNamespace + ".CreateDatabase.sql"))) {
				schemaSqlBuilder.Append(sr.ReadToEnd());
			}
			foreach (string remove in removeSnippets) {
				schemaSqlBuilder.Replace(remove, string.Empty);
			}
			schemaSqlBuilder.Replace("Path1_Placeholder", HttpContext.Current.Server.MapPath("~/App_Data/"));
			schemaSqlBuilder.Replace("WEBROOT", databasePath);
			schemaSqlBuilder.Replace("$(DatabaseName)", databaseName);

			string sql = string.Format(CultureInfo.InvariantCulture, SqlFormat, schemaSqlBuilder, claimedId, "Admin");

			var serverConnection = new ServerConnection(".\\sqlexpress");
			try {
				serverConnection.ExecuteNonQuery(sql);
			} finally {
				try {
					var server = new Server(serverConnection);
					server.DetachDatabase(databaseName, true);
				} catch (FailedOperationException) {
				}
				serverConnection.Disconnect();
			}
		}

		public static int ExecuteCommand(this ObjectContext objectContext, string command) {
			// Try to automatically add the appropriate transaction if one is known.
			EntityTransaction transaction = null;
			if (Database.IsDataContextInitialized && Database.DataContext == objectContext) {
				transaction = Database.DataContextTransaction;
			}
			return ExecuteCommand(objectContext, transaction, command);
		}

		/// <summary>
		/// Executes a SQL command against the SQL connection.
		/// </summary>
		/// <param name="objectContext">The object context.</param>
		/// <param name="transaction">The transaction to use, if any.</param>
		/// <param name="command">The command to execute.</param>
		/// <returns>The result of executing the command.</returns>
		public static int ExecuteCommand(this ObjectContext objectContext, EntityTransaction transaction, string command) {
			if (objectContext == null) {
				throw new ArgumentNullException("objectContext");
			}
			if (string.IsNullOrEmpty(command)) {
				throw new ArgumentNullException("command");
			}

			DbConnection connection = (EntityConnection)objectContext.Connection;
			bool opening = connection.State == ConnectionState.Closed;
			if (opening) {
				connection.Open();
			}

			DbCommand cmd = connection.CreateCommand();
			cmd.Transaction = transaction;
			cmd.CommandText = command;
			cmd.CommandType = CommandType.StoredProcedure;
			try {
				return cmd.ExecuteNonQuery();
			} finally {
				if (opening && connection.State == ConnectionState.Open) {
					connection.Close();
				}
			}
		}

		internal static void VerifyThrowNotLocalTime(DateTime value) {
			// When we want UTC time, we have to accept Unspecified kind
			// because that's how it is set to us in the database.
			if (value.Kind == DateTimeKind.Local) {
				throw new ArgumentException("DateTime must be given in UTC time but was " + value.Kind.ToString());
			}
		}

		/// <summary>
		/// Ensures that local times are converted to UTC times.  Unspecified kinds are recast to UTC with no conversion.
		/// </summary>
		/// <param name="value">The date-time to convert.</param>
		/// <returns>The date-time in UTC time.</returns>
		internal static DateTime AsUtc(this DateTime value) {
			if (value.Kind == DateTimeKind.Unspecified) {
				return new DateTime(value.Ticks, DateTimeKind.Utc);
			}

			return value.ToUniversalTime();
		}
	}
}
