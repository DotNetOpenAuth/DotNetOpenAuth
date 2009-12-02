//-----------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using System.Data.EntityClient;
	using System.Data.Objects;
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
CREATE DATABASE [{0}] ON (NAME='{0}', FILENAME='{0}')
GO
USE ""{0}""
GO
{1}
EXEC [dbo].[AddUser] 'admin', 'admin', '{2}', '{3}'
GO
";
			string schemaSql;
			using (var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(DefaultNamespace + ".CreateDatabase.sql"))) {
				schemaSql = sr.ReadToEnd();
			}
			string databasePath = HttpContext.Current.Server.MapPath("~/App_Data/" + databaseName + ".mdf");
			string sql = string.Format(CultureInfo.InvariantCulture, SqlFormat, databasePath, schemaSql, claimedId, "Admin");

			var serverConnection = new ServerConnection(".\\sqlexpress");
			try {
				serverConnection.ExecuteNonQuery(sql);
				var server = new Server(serverConnection);
				server.DetachDatabase(databasePath, true);
			} finally {
				serverConnection.Disconnect();
			}
		}

		/// <summary>
		/// Executes a SQL command against the SQL connection.
		/// </summary>
		/// <param name="objectContext">The object context.</param>
		/// <param name="command">The command to execute.</param>
		/// <returns>The result of executing the command.</returns>
		public static int ExecuteCommand(this ObjectContext objectContext, string command) {
			DbConnection connection = ((EntityConnection)objectContext.Connection).StoreConnection;
			bool opening = (connection.State == ConnectionState.Closed);
			if (opening) {
				connection.Open();
			}

			DbCommand cmd = connection.CreateCommand();
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
	}
}
