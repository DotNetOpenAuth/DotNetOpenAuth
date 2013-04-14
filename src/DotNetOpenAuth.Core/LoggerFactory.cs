//-----------------------------------------------------------------------
// <copyright file="LoggerFactory.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Globalization;
	using DotNetOpenAuth.Loggers;
	using DotNetOpenAuth.Messaging;
	using log4net.Core;
	using Validation;

	/// <summary>
	/// A general logger for the entire DotNetOpenAuth library.
	/// </summary>
	/// <remarks>
	/// Because this logger is intended for use with non-localized strings, the
	/// overloads that take <see cref="CultureInfo" /> have been removed, and
	/// <see cref="CultureInfo.InvariantCulture" /> is used implicitly.
	/// </remarks>
	internal class LoggerFactory : ILoggerFactory {
		/// <summary>
		/// Creates a logger.
		/// </summary>
		/// <param name="name">The name for the logger.</param>
		/// <returns>
		/// The instantiated logger.
		/// </returns>
		public ILog CreateLogger(string name) {
			return Create(name);
		}

		/// <summary>
		/// Creates an additional logger on demand for a subsection of the application.
		/// </summary>
		/// <param name="name">A name that will be included in the log file.</param>
		/// <returns>The <see cref="ILog"/> instance created with the given name.</returns>
		internal static ILog Create(string name) {
			Requires.NotNullOrEmpty(name, "name");
			return InitializeFacade(name);
		}

		/// <summary>
		/// Creates the main logger for the library, and emits an INFO message
		/// that is the name and version of the library.
		/// </summary>
		/// <param name="name">A name that will be included in the log file.</param>
		/// <returns>The <see cref="ILog"/> instance created with the given name.</returns>
		internal static ILog CreateWithBanner(string name) {
			Requires.NotNullOrEmpty(name, "name");
			ILog log = Create(name);
			log.Info(PortableUtilities.LibraryVersion);
			return log;
		}

		/// <summary>
		/// Creates an additional logger on demand for a subsection of the application.
		/// </summary>
		/// <param name="type">A type whose full name that will be included in the log file.</param>
		/// <returns>The <see cref="ILog"/> instance created with the given type name.</returns>
		internal static ILog Create(Type type) {
			Requires.NotNull(type, "type");

			return Create(type.FullName);
		}

		/// <summary>
		/// Discovers the presence of Log4net.dll and other logging mechanisms
		/// and returns the best available logger.
		/// </summary>
		/// <param name="name">The name of the log to initialize.</param>
		/// <returns>The <see cref="ILog"/> instance of the logger to use.</returns>
		private static ILog InitializeFacade(string name) {
			ILog result = Log4NetLogger.Initialize(name) ?? NLogLogger.Initialize(name) ?? TraceLogger.Initialize(name) ?? NoOpLogger.Initialize();
			return result;
		}
	}
}
