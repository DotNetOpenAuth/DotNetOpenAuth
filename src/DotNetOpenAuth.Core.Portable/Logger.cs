//-----------------------------------------------------------------------
// <copyright file="Logger.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System.Globalization;

	using DotNetOpenAuth.Loggers;
	using Validation;

	/// <summary>
	/// A general logger for the entire DotNetOpenAuth library.
	/// </summary>
	/// <remarks>
	/// Because this logger is intended for use with non-localized strings, the
	/// overloads that take <see cref="CultureInfo"/> have been removed, and 
	/// <see cref="CultureInfo.InvariantCulture"/> is used implicitly.
	/// </remarks>
	public static class Logger {
		#region Category-specific loggers

		/// <summary>
		/// The <see cref="ILog"/> instance that is to be used 
		/// by this static Logger for the duration of the appdomain.
		/// </summary>
		private static ILog library = NoOpLogger.Initialize();

		/// <summary>
		/// Backing field for the <see cref="Yadis"/> property.
		/// </summary>
		private static ILog yadis = NoOpLogger.Initialize();

		/// <summary>
		/// Backing field for the <see cref="Messaging"/> property.
		/// </summary>
		private static ILog messaging = NoOpLogger.Initialize();

		/// <summary>
		/// Backing field for the <see cref="Channel"/> property.
		/// </summary>
		private static ILog channel = NoOpLogger.Initialize();

		/// <summary>
		/// Backing field for the <see cref="Bindings"/> property.
		/// </summary>
		private static ILog bindings = NoOpLogger.Initialize();

		/// <summary>
		/// Backing field for the <see cref="Signatures"/> property.
		/// </summary>
		private static ILog signatures = NoOpLogger.Initialize();

		/// <summary>
		/// Backing field for the <see cref="Http"/> property.
		/// </summary>
		private static ILog http = NoOpLogger.Initialize();

		/// <summary>
		/// Backing field for the <see cref="Controls"/> property.
		/// </summary>
		private static ILog controls = NoOpLogger.Initialize();

		/// <summary>
		/// Backing field for the <see cref="OpenId"/> property.
		/// </summary>
		private static ILog openId = NoOpLogger.Initialize();

		/// <summary>
		/// Backing field for the <see cref="OAuth"/> property.
		/// </summary>
		private static ILog oauth = NoOpLogger.Initialize();

		/// <summary>
		/// Backing field for the <see cref="InfoCard"/> property.
		/// </summary>
		private static ILog infocard = NoOpLogger.Initialize();

		/// <summary>
		/// Gets the logger for general library logging.
		/// </summary>
		internal static ILog Library { get { return library; } }

		/// <summary>
		/// Gets the logger for service discovery and selection events.
		/// </summary>
		internal static ILog Yadis { get { return yadis; } }

		/// <summary>
		/// Gets the logger for Messaging events.
		/// </summary>
		internal static ILog Messaging { get { return messaging; } }

		/// <summary>
		/// Gets the logger for Channel events.
		/// </summary>
		internal static ILog Channel { get { return channel; } }

		/// <summary>
		/// Gets the logger for binding elements and binding-element related events on the channel.
		/// </summary>
		internal static ILog Bindings { get { return bindings; } }

		/// <summary>
		/// Gets the logger specifically used for logging verbose text on everything about the signing process.
		/// </summary>
		internal static ILog Signatures { get { return signatures; } }

		/// <summary>
		/// Gets the logger for HTTP-level events.
		/// </summary>
		internal static ILog Http { get { return http; } }

		/// <summary>
		/// Gets the logger for events logged by ASP.NET controls.
		/// </summary>
		internal static ILog Controls { get { return controls; } }

		/// <summary>
		/// Gets the logger for high-level OpenID events.
		/// </summary>
		internal static ILog OpenId { get { return openId; } }

		/// <summary>
		/// Gets the logger for high-level OAuth events.
		/// </summary>
		internal static ILog OAuth { get { return oauth; } }

		/// <summary>
		/// Gets the logger for high-level InfoCard events.
		/// </summary>
		internal static ILog InfoCard { get { return infocard; } }

		#endregion

		/// <summary>
		/// Initializes logging using the specified factory.
		/// </summary>
		/// <param name="loggerFactory">The logger factory.</param>
		public static void InitializeLogging(ILoggerFactory loggerFactory) {
			Requires.NotNull(loggerFactory, "loggerFactory");

			library = loggerFactory.CreateLogger("DotNetOpenAuth");
			yadis = loggerFactory.CreateLogger("DotNetOpenAuth.Yadis");
			messaging = loggerFactory.CreateLogger("DotNetOpenAuth.Messaging");
			channel = loggerFactory.CreateLogger("DotNetOpenAuth.Messaging.Channel");
			bindings = loggerFactory.CreateLogger("DotNetOpenAuth.Messaging.Bindings");
			signatures = loggerFactory.CreateLogger("DotNetOpenAuth.Messaging.Bindings.Signatures");
			http = loggerFactory.CreateLogger("DotNetOpenAuth.Http");
			controls = loggerFactory.CreateLogger("DotNetOpenAuth.Controls");
			openId = loggerFactory.CreateLogger("DotNetOpenAuth.OpenId");
			oauth = loggerFactory.CreateLogger("DotNetOpenAuth.OAuth");
			infocard = loggerFactory.CreateLogger("DotNetOpenAuth.InfoCard");
		}
	}
}
