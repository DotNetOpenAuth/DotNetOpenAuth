using System;
using System.Globalization;
using YOURLIBNAME.Loggers;

namespace YOURLIBNAME {
	/// <summary>
	/// A general logger for the entire YOURLIBNAME library.
	/// </summary>
	/// <remarks>
	/// Because this logger is intended for use with non-localized strings, the
	/// overloads that take <see cref="CultureInfo"/> have been removed, and 
	/// <see cref="CultureInfo.InvariantCulture"/> is used implicitly.
	/// </remarks>
	static class Logger {
		static ILog facade = initializeFacade();

		static ILog initializeFacade() {
			ILog result = Log4NetLogger.Initialize() ?? TraceLogger.Initialize() ?? NoOpLogger.Initialize();
			result.Info(Util.LibraryVersion);
			return result;
		}

		#region ILog Members
		// Although this static class doesn't literally implement the ILog interface, 
		// we implement (mostly) all the same methods in a static way.

		public static void Debug(object message) {
			facade.Debug(message);
		}

		public static void Debug(object message, Exception exception) {
			facade.Debug(message, exception);
		}

		public static void DebugFormat(string format, params object[] args) {
			facade.DebugFormat(CultureInfo.InvariantCulture, format, args);
		}

		public static void DebugFormat(string format, object arg0) {
			facade.DebugFormat(format, arg0);
		}

		public static void DebugFormat(string format, object arg0, object arg1) {
			facade.DebugFormat(format, arg0, arg1);
		}

		public static void DebugFormat(string format, object arg0, object arg1, object arg2) {
			facade.DebugFormat(format, arg0, arg1, arg2);
		}

		/*
		public static void DebugFormat(IFormatProvider provider, string format, params object[] args) {
			facade.DebugFormat(provider, format, args);
		}
		*/

		public static void Info(object message) {
			facade.Info(message);
		}

		public static void Info(object message, Exception exception) {
			facade.Info(message, exception);
		}

		public static void InfoFormat(string format, params object[] args) {
			facade.InfoFormat(CultureInfo.InvariantCulture, format, args);
		}

		public static void InfoFormat(string format, object arg0) {
			facade.InfoFormat(format, arg0);
		}

		public static void InfoFormat(string format, object arg0, object arg1) {
			facade.InfoFormat(format, arg0, arg1);
		}

		public static void InfoFormat(string format, object arg0, object arg1, object arg2) {
			facade.InfoFormat(format, arg0, arg1, arg2);
		}

		/*
		public static void InfoFormat(IFormatProvider provider, string format, params object[] args) {
			facade.InfoFormat(provider, format, args);
		}
		*/

		public static void Warn(object message) {
			facade.Warn(message);
		}

		public static void Warn(object message, Exception exception) {
			facade.Warn(message, exception);
		}

		public static void WarnFormat(string format, params object[] args) {
			facade.WarnFormat(CultureInfo.InvariantCulture, format, args);
		}

		public static void WarnFormat(string format, object arg0) {
			facade.WarnFormat(format, arg0);
		}

		public static void WarnFormat(string format, object arg0, object arg1) {
			facade.WarnFormat(format, arg0, arg1);
		}

		public static void WarnFormat(string format, object arg0, object arg1, object arg2) {
			facade.WarnFormat(format, arg0, arg1, arg2);
		}

		/*
		public static void WarnFormat(IFormatProvider provider, string format, params object[] args) {
			facade.WarnFormat(provider, format, args);
		}
		*/

		public static void Error(object message) {
			facade.Error(message);
		}

		public static void Error(object message, Exception exception) {
			facade.Error(message, exception);
		}

		public static void ErrorFormat(string format, params object[] args) {
			facade.ErrorFormat(CultureInfo.InvariantCulture, format, args);
		}

		public static void ErrorFormat(string format, object arg0) {
			facade.ErrorFormat(format, arg0);
		}

		public static void ErrorFormat(string format, object arg0, object arg1) {
			facade.ErrorFormat(format, arg0, arg1);
		}

		public static void ErrorFormat(string format, object arg0, object arg1, object arg2) {
			facade.ErrorFormat(format, arg0, arg1, arg2);
		}

		/*
		public static void ErrorFormat(IFormatProvider provider, string format, params object[] args) {
			facade.ErrorFormat(provider, format, args);
		}
		*/

		public static void Fatal(object message) {
			facade.Fatal(message);
		}

		public static void Fatal(object message, Exception exception) {
			facade.Fatal(message, exception);
		}

		public static void FatalFormat(string format, params object[] args) {
			facade.FatalFormat(CultureInfo.InvariantCulture, format, args);
		}

		public static void FatalFormat(string format, object arg0) {
			facade.FatalFormat(format, arg0);
		}

		public static void FatalFormat(string format, object arg0, object arg1) {
			facade.FatalFormat(format, arg0, arg1);
		}

		public static void FatalFormat(string format, object arg0, object arg1, object arg2) {
			facade.FatalFormat(format, arg0, arg1, arg2);
		}

		/*
		public static void FatalFormat(IFormatProvider provider, string format, params object[] args) {
			facade.FatalFormat(provider, format, args);
		}
		*/

		public static bool IsDebugEnabled {
			get { return facade.IsDebugEnabled; }
		}

		public static bool IsInfoEnabled {
			get { return facade.IsInfoEnabled; }
		}

		public static bool IsWarnEnabled {
			get { return facade.IsWarnEnabled; }
		}

		public static bool IsErrorEnabled {
			get { return facade.IsErrorEnabled; }
		}

		public static bool IsFatalEnabled {
			get { return facade.IsFatalEnabled; }
		}

		#endregion
	}
}
