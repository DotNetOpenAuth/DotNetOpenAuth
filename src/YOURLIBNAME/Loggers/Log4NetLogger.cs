namespace YOURLIBNAME.Loggers {
	using System;
	using System.Globalization;
	using System.IO;
	using System.Reflection;

	class Log4NetLogger : ILog {
		private log4net.ILog log4netLogger;

		private Log4NetLogger(log4net.ILog logger) {
			log4netLogger = logger;
		}

		static bool IsLog4NetPresent {
			get {
				try {
					Assembly.Load("log4net");
					return true;
				} catch (FileNotFoundException) {
					return false;
				}
			}
		}

		#region ILog Members

		public bool IsDebugEnabled {
			get { return log4netLogger.IsDebugEnabled; }
		}

		public bool IsInfoEnabled {
			get { return log4netLogger.IsInfoEnabled; }
		}

		public bool IsWarnEnabled {
			get { return log4netLogger.IsWarnEnabled; }
		}

		public bool IsErrorEnabled {
			get { return log4netLogger.IsErrorEnabled; }
		}

		public bool IsFatalEnabled {
			get { return log4netLogger.IsFatalEnabled; }
		}

		public void Debug(object message) {
			log4netLogger.Debug(message);
		}

		public void Debug(object message, Exception exception) {
			log4netLogger.Debug(message, exception);
		}

		public void DebugFormat(string format, params object[] args) {
			log4netLogger.DebugFormat(CultureInfo.InvariantCulture, format, args);
		}

		public void DebugFormat(string format, object arg0) {
			log4netLogger.DebugFormat(format, arg0);
		}

		public void DebugFormat(string format, object arg0, object arg1) {
			log4netLogger.DebugFormat(format, arg0, arg1);
		}

		public void DebugFormat(string format, object arg0, object arg1, object arg2) {
			log4netLogger.DebugFormat(format, arg0, arg1, arg2);
		}

		public void DebugFormat(IFormatProvider provider, string format, params object[] args) {
			log4netLogger.DebugFormat(provider, format, args);
		}

		public void Info(object message) {
			log4netLogger.Info(message);
		}

		public void Info(object message, Exception exception) {
			log4netLogger.Info(message, exception);
		}

		public void InfoFormat(string format, params object[] args) {
			log4netLogger.InfoFormat(CultureInfo.InvariantCulture, format, args);
		}

		public void InfoFormat(string format, object arg0) {
			log4netLogger.InfoFormat(format, arg0);
		}

		public void InfoFormat(string format, object arg0, object arg1) {
			log4netLogger.InfoFormat(format, arg0, arg1);
		}

		public void InfoFormat(string format, object arg0, object arg1, object arg2) {
			log4netLogger.InfoFormat(format, arg0, arg1, arg2);
		}

		public void InfoFormat(IFormatProvider provider, string format, params object[] args) {
			log4netLogger.InfoFormat(provider, format, args);
		}

		public void Warn(object message) {
			log4netLogger.Warn(message);
		}

		public void Warn(object message, Exception exception) {
			log4netLogger.Warn(message, exception);
		}

		public void WarnFormat(string format, params object[] args) {
			log4netLogger.WarnFormat(CultureInfo.InvariantCulture, format, args);
		}

		public void WarnFormat(string format, object arg0) {
			log4netLogger.WarnFormat(format, arg0);
		}

		public void WarnFormat(string format, object arg0, object arg1) {
			log4netLogger.WarnFormat(format, arg0, arg1);
		}

		public void WarnFormat(string format, object arg0, object arg1, object arg2) {
			log4netLogger.WarnFormat(format, arg0, arg1, arg2);
		}

		public void WarnFormat(IFormatProvider provider, string format, params object[] args) {
			log4netLogger.WarnFormat(provider, format, args);
		}

		public void Error(object message) {
			log4netLogger.Error(message);
		}

		public void Error(object message, Exception exception) {
			log4netLogger.Error(message, exception);
		}

		public void ErrorFormat(string format, params object[] args) {
			log4netLogger.ErrorFormat(CultureInfo.InvariantCulture, format, args);
		}

		public void ErrorFormat(string format, object arg0) {
			log4netLogger.ErrorFormat(format, arg0);
		}

		public void ErrorFormat(string format, object arg0, object arg1) {
			log4netLogger.ErrorFormat(format, arg0, arg1);
		}

		public void ErrorFormat(string format, object arg0, object arg1, object arg2) {
			log4netLogger.ErrorFormat(format, arg0, arg1, arg2);
		}

		public void ErrorFormat(IFormatProvider provider, string format, params object[] args) {
			log4netLogger.ErrorFormat(provider, format, args);
		}

		public void Fatal(object message) {
			log4netLogger.Fatal(message);
		}

		public void Fatal(object message, Exception exception) {
			log4netLogger.Fatal(message, exception);
		}

		public void FatalFormat(string format, params object[] args) {
			log4netLogger.FatalFormat(CultureInfo.InvariantCulture, format, args);
		}

		public void FatalFormat(string format, object arg0) {
			log4netLogger.FatalFormat(format, arg0);
		}

		public void FatalFormat(string format, object arg0, object arg1) {
			log4netLogger.FatalFormat(format, arg0, arg1);
		}

		public void FatalFormat(string format, object arg0, object arg1, object arg2) {
			log4netLogger.FatalFormat(format, arg0, arg1, arg2);
		}

		public void FatalFormat(IFormatProvider provider, string format, params object[] args) {
			log4netLogger.FatalFormat(provider, format, args);
		}

		#endregion

		/// <summary>
		/// Returns a new log4net logger if it exists, or returns null if the assembly cannot be found.
		/// </summary>
		/// <returns>The created <see cref="ILog"/> instance.</returns>
		internal static ILog Initialize() {
			return IsLog4NetPresent ? CreateLogger() : null;
		}

		/// <summary>
		/// Creates the log4net.LogManager.  Call ONLY after log4net.dll is known to be present.
		/// </summary>
		/// <returns>The created <see cref="ILog"/> instance.</returns>
		static ILog CreateLogger() {
			return new Log4NetLogger(log4net.LogManager.GetLogger("YOURLIBNAME"));
		}
	}
}
