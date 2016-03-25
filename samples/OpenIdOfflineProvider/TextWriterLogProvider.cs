//-----------------------------------------------------------------------
// <copyright file="TextWriterLogProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Threading;

	using DotNetOpenAuth.Logging;

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
	internal class TextWriterLogProvider : ILogProvider {
		private readonly TextBoxTextWriter _writer;

		private static bool _providerIsAvailableOverride = true;

		internal TextWriterLogProvider(TextBoxTextWriter writer) {
			this._writer = writer;
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
			return new TextWriterLogger(this._writer);
		}

		public class TextWriterLogger : ILog {
			private readonly int _skipLevel;
			private TextWriter _writer;
			private ITextWriterFormatter _textWriterFormatter;
			private IDateTimeProvider _dateTimeProvider;

			internal TextWriterLogger(TextBoxTextWriter writer) {
				this._writer = writer;
				this._skipLevel = 1;
				this.DateTimeProvider = new UtcDateTimeProvider();
				this.TextWriterFormatter = new DefaultTextWriterFormatter();
			}

			public interface IDateTimeProvider {
				DateTime GetCurrentDateTime();
			}

			public interface ITextWriterFormatter {
				void WriteText(TextWriter writer, LogLevel level, DateTime dateTime, string text);
			}

			public TextWriter Writer {
				get {
					return this._writer;
				}

				set {
					this._writer = value;
				}
			}

			public ITextWriterFormatter TextWriterFormatter {
				get {
					return this._textWriterFormatter;
				}

				set {
					if (value == null) {
						throw new ArgumentNullException();
					}
					this._textWriterFormatter = value;
				}
			}

			public IDateTimeProvider DateTimeProvider {
				get {
					return this._dateTimeProvider;
				}

				set {
					if (value == null) {
						throw new ArgumentNullException();
					}
					this._dateTimeProvider = value;
				}
			}

			public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception) {
				if (messageFunc == null) {
					// nothing to log..
					return true;
				}
				string[] lines = Regex.Split(messageFunc(), "\r\n|\r|\n");
				foreach (var line in lines) {
					this.TextWriterFormatter.WriteText(this._writer, logLevel, this.DateTimeProvider.GetCurrentDateTime(), line);
				}
				this._writer.Flush();
				return true;
			}

			public class DefaultTextWriterFormatter : ITextWriterFormatter {
				public void WriteText(TextWriter writer, LogLevel level, DateTime dateTime, string text) {
					string logLevelAbbreviation;
					switch (level) {
						case LogLevel.Info:
						default:
							logLevelAbbreviation = "I";
							break;

						case LogLevel.Debug:
							logLevelAbbreviation = "D";
							break;

						case LogLevel.Warn:
							logLevelAbbreviation = "W";
							break;

						case LogLevel.Error:
							logLevelAbbreviation = "E";
							break;
					}

					writer.WriteLine(
						"{0}:{1} {2} [{4}]: {3}",
						logLevelAbbreviation,
						dateTime.ToShortDateString(),
						dateTime.ToLongTimeString(),
						text,
						Thread.CurrentThread.GetHashCode());
				}
			}

			public class UtcDateTimeProvider : IDateTimeProvider {
				public DateTime GetCurrentDateTime() {
					return DateTime.UtcNow;
				}
			}
		}
	}
}
