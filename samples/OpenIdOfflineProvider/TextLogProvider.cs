using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel.Dispatcher;

using DotNetOpenAuth.Logging;
using DotNetOpenAuth.Logging.LogProviders;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;

namespace DotNetOpenAuth.OpenIdOfflineProvider {

	/// <summary>
	/// Sends logging events to a <see cref="TextWriter"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// An Appender that writes to a <see cref="TextWriter"/>.
	/// </para>
	/// <para>
	/// This appender may be used stand alone if initialized with an appropriate
	/// writer, however it is typically used as a base class for an appender that
	/// can open a <see cref="TextWriter"/> to write to.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	/// <author>Douglas de la Torre</author>
	public class TextWriterLogProvider : ILogProvider {

		private static bool _providerIsAvailableOverride = true;
		private readonly TextWriterLogger.WriteDelegate _logWriteDelegate;

		public TextWriterLogProvider() {
			if (!IsLoggerAvailable()) {
				throw new InvalidOperationException("Gibraltar.Agent.Log (Loupe) not found");
			}

			_logWriteDelegate = GetLogWriteDelegate();
		}

		/// <summary>
		/// Gets or sets a value indicating whether [provider is available override]. Used in tests.
		/// </summary>
		/// <value>
		/// <c>true</c> if [provider is available override]; otherwise, <c>false</c>.
		/// </value>
		public static bool ProviderIsAvailableOverride {
			get { return _providerIsAvailableOverride; }
			set { _providerIsAvailableOverride = value; }
		}

		public ILog GetLogger(string name) {
			return new TextWriterLogProvider.TextWriterLogger(name, _logWriteDelegate);
		}

		public static bool IsLoggerAvailable() {
			return ProviderIsAvailableOverride && GetLogManagerType() != null;
		}

		private static Type GetLogManagerType() {
			return Type.GetType("Gibraltar.Agent.Log, Gibraltar.Agent");
		}

		private static TextWriterLogger.WriteDelegate GetLogWriteDelegate() {
			Type logManagerType = GetLogManagerType();
			Type logMessageSeverityType = Type.GetType("Gibraltar.Agent.LogMessageSeverity, Gibraltar.Agent");
			Type logWriteModeType = Type.GetType("Gibraltar.Agent.LogWriteMode, Gibraltar.Agent");

			MethodInfo method = logManagerType.GetMethod("Write", new[]
                                                                  {
                                                                      logMessageSeverityType, typeof(string), typeof(int), typeof(Exception), typeof(bool), 
                                                                      logWriteModeType, typeof(string), typeof(string), typeof(string), typeof(string), typeof(object[])
                                                                  });

			var callDelegate = (TextWriterLogger.WriteDelegate)Delegate.CreateDelegate(typeof(TextWriterLogger.WriteDelegate), method);
			return callDelegate;
		}
		public class TextWriterLogger : ILog {
			private const string LogSystem = "LibLog";

			private readonly string _category;
			private readonly WriteDelegate _logWriteDelegate;
			private readonly int _skipLevel;

			internal TextWriterLogger(string category, WriteDelegate logWriteDelegate) {
				_category = category;
				_logWriteDelegate = logWriteDelegate;
				_skipLevel = 1;
			}

			public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception) {
				if (messageFunc == null) {
					//nothing to log..
					return true;
				}

				_logWriteDelegate((int)ToLogMessageSeverity(logLevel), LogSystem, _skipLevel, exception, true, 0, null, _category, null, messageFunc.Invoke());

				return true;
			}

			public TraceEventType ToLogMessageSeverity(LogLevel logLevel) {
				switch (logLevel) {
					case LogLevel.Trace:
						return TraceEventType.Verbose;
					case LogLevel.Debug:
						return TraceEventType.Verbose;
					case LogLevel.Info:
						return TraceEventType.Information;
					case LogLevel.Warn:
						return TraceEventType.Warning;
					case LogLevel.Error:
						return TraceEventType.Error;
					case LogLevel.Fatal:
						return TraceEventType.Critical;
					default:
						throw new ArgumentOutOfRangeException("logLevel");
				}
			}

			/// <summary>
			/// The form of the Loupe Log.Write method we're using
			/// </summary>
			internal delegate void WriteDelegate(
				int severity,
				string logSystem,
				int skipFrames,
				Exception exception,
				bool attributeToException,
				int writeMode,
				string detailsXml,
				string category,
				string caption,
				string description,
				params object[] args
				);
		}
	}
}
