//-----------------------------------------------------------------------
// <copyright file="Reporting.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.IO.IsolatedStorage;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Reflection;
	using System.Security;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using Validation;

	/// <summary>
	/// The statistical reporting mechanism used so this library's project authors
	/// know what versions and features are in use.
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "This class is derived from, so it can't be static.")]
	public class Reporting {
		/// <summary>
		/// A UTF8 encoder that doesn't emit the preamble.  Used for mid-stream writers.
		/// </summary>
		private static readonly Encoding Utf8NoPreamble = new UTF8Encoding(false);

		/// <summary>
		/// A value indicating whether reporting is desirable or not.  Must be logical-AND'd with !<see cref="broken"/>.
		/// </summary>
		private static bool enabled;

		/// <summary>
		/// A value indicating whether reporting experienced an error and cannot be enabled.
		/// </summary>
		private static bool broken;

		/// <summary>
		/// A value indicating whether the reporting class has been initialized or not.
		/// </summary>
		private static bool initialized;

		/// <summary>
		/// The object to lock during initialization.
		/// </summary>
		private static object initializationSync = new object();

		/// <summary>
		/// The isolated storage to use for collecting data in between published reports.
		/// </summary>
		private static IsolatedStorageFile file;

		/// <summary>
		/// The GUID that shows up at the top of all reports from this user/machine/domain.
		/// </summary>
		private static Guid reportOriginIdentity;

		/// <summary>
		/// The recipient of collected reports.
		/// </summary>
		private static Uri wellKnownPostLocation = new Uri("https://reports.dotnetopenauth.net/ReportingPost.ashx");

		/// <summary>
		/// A few HTTP request hosts and paths we've seen.
		/// </summary>
		private static PersistentHashSet observedRequests;

		/// <summary>
		/// Cultures that have come in via HTTP requests.
		/// </summary>
		private static PersistentHashSet observedCultures;

		/// <summary>
		/// Features that have been used.
		/// </summary>
		private static PersistentHashSet observedFeatures;

		/// <summary>
		/// A collection of all the observations to include in the report.
		/// </summary>
		private static List<PersistentHashSet> observations = new List<PersistentHashSet>();

		/// <summary>
		/// The named events that we have counters for.
		/// </summary>
		private static Dictionary<string, PersistentCounter> events = new Dictionary<string, PersistentCounter>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// The lock acquired while considering whether to publish a report.
		/// </summary>
		private static object publishingConsiderationLock = new object();

		/// <summary>
		/// The time that we last published reports.
		/// </summary>
		private static DateTime lastPublished = DateTime.Now;

		/// <summary>
		/// Initializes static members of the <see cref="Reporting"/> class.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We do more than field initialization here.")]
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Reporting MUST NOT cause unhandled exceptions.")]
		static Reporting() {
			Enabled = DotNetOpenAuthSection.Reporting.Enabled;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Reporting"/> class.
		/// </summary>
		protected Reporting() {
		}

		/// <summary>
		/// Gets or sets a value indicating whether this reporting is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Setting this property to <c>true</c> <i>may</i> have no effect
		/// if reporting has already experienced a failure of some kind.
		/// </remarks>
		public static bool Enabled {
			get {
				return enabled && !broken;
			}

			set {
				if (value) {
					Initialize();
				}

				// Only set the static field here, so that other threads
				// don't try to use reporting while we're initializing it.
				enabled = value;
			}
		}

		/// <summary>
		/// Gets the observed features.
		/// </summary>
		internal static PersistentHashSet ObservedFeatures {
			get { return observedFeatures; }
		}

		/// <summary>
		/// Gets the configuration to use for reporting.
		/// </summary>
		internal static ReportingElement Configuration {
			get { return DotNetOpenAuthSection.Reporting; }
		}

		/// <summary>
		/// Records an event occurrence.
		/// </summary>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="category">The category within the event.  Null and empty strings are allowed, but considered the same.</param>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "PersistentCounter instances are stored in a table for later use.")]
		internal static void RecordEventOccurrence(string eventName, string category) {
			Requires.NotNullOrEmpty(eventName, "eventName");

			// In release builds, just quietly return.
			if (string.IsNullOrEmpty(eventName)) {
				return;
			}

			if (Enabled && Configuration.IncludeEventStatistics) {
				PersistentCounter counter;
				lock (events) {
					if (!events.TryGetValue(eventName, out counter)) {
						events[eventName] = counter = new PersistentCounter(file, "event-" + SanitizeFileName(eventName) + ".txt");
					}
				}

				counter.Increment(category);
				Touch();
			}
		}

		/// <summary>
		/// Records an event occurence.
		/// </summary>
		/// <param name="eventNameByObjectType">The object whose type name is the event name to record.</param>
		/// <param name="category">The category within the event.  Null and empty strings are allowed, but considered the same.</param>
		internal static void RecordEventOccurrence(object eventNameByObjectType, string category) {
			Requires.NotNull(eventNameByObjectType, "eventNameByObjectType");

			// In release builds, just quietly return.
			if (eventNameByObjectType == null) {
				return;
			}

			if (Enabled && Configuration.IncludeEventStatistics) {
				RecordEventOccurrence(eventNameByObjectType.GetType().Name, category);
			}
		}

		/// <summary>
		/// Records the use of a feature by name.
		/// </summary>
		/// <param name="feature">The feature.</param>
		internal static void RecordFeatureUse(string feature) {
			Requires.NotNullOrEmpty(feature, "feature");

			// In release builds, just quietly return.
			if (string.IsNullOrEmpty(feature)) {
				return;
			}

			if (Enabled && Configuration.IncludeFeatureUsage) {
				observedFeatures.Add(feature);
				Touch();
			}
		}

		/// <summary>
		/// Records the use of a feature by object type.
		/// </summary>
		/// <param name="value">The object whose type is the feature to set as used.</param>
		internal static void RecordFeatureUse(object value) {
			Requires.NotNull(value, "value");

			// In release builds, just quietly return.
			if (value == null) {
				return;
			}

			if (Enabled && Configuration.IncludeFeatureUsage) {
				observedFeatures.Add(value.GetType().Name);
				Touch();
			}
		}

		/// <summary>
		/// Records the use of a feature by object type.
		/// </summary>
		/// <param name="value">The object whose type is the feature to set as used.</param>
		/// <param name="dependency1">Some dependency used by <paramref name="value"/>.</param>
		internal static void RecordFeatureAndDependencyUse(object value, object dependency1) {
			Requires.NotNull(value, "value");

			// In release builds, just quietly return.
			if (value == null) {
				return;
			}

			if (Enabled && Configuration.IncludeFeatureUsage) {
				StringBuilder builder = new StringBuilder();
				builder.Append(value.GetType().Name);
				builder.Append(" ");
				builder.Append(dependency1 != null ? dependency1.GetType().Name : "(null)");
				observedFeatures.Add(builder.ToString());
				Touch();
			}
		}

		/// <summary>
		/// Records the use of a feature by object type.
		/// </summary>
		/// <param name="value">The object whose type is the feature to set as used.</param>
		/// <param name="dependency1">Some dependency used by <paramref name="value"/>.</param>
		/// <param name="dependency2">Some dependency used by <paramref name="value"/>.</param>
		internal static void RecordFeatureAndDependencyUse(object value, object dependency1, object dependency2) {
			Requires.NotNull(value, "value");

			// In release builds, just quietly return.
			if (value == null) {
				return;
			}

			if (Enabled && Configuration.IncludeFeatureUsage) {
				StringBuilder builder = new StringBuilder();
				builder.Append(value.GetType().Name);
				builder.Append(" ");
				builder.Append(dependency1 != null ? dependency1.GetType().Name : "(null)");
				builder.Append(" ");
				builder.Append(dependency2 != null ? dependency2.GetType().Name : "(null)");
				observedFeatures.Add(builder.ToString());
				Touch();
			}
		}

		/// <summary>
		/// Records statistics collected from incoming requests.
		/// </summary>
		/// <param name="request">The request.</param>
		internal static void RecordRequestStatistics(HttpRequestBase request) {
			Requires.NotNull(request, "request");

			// In release builds, just quietly return.
			if (request == null) {
				return;
			}

			if (Enabled) {
				if (Configuration.IncludeCultures) {
					observedCultures.Add(Thread.CurrentThread.CurrentCulture.Name);
				}

				if (Configuration.IncludeLocalRequestUris && !observedRequests.IsFull) {
					var requestBuilder = new UriBuilder(request.GetPublicFacingUrl());
					requestBuilder.Query = null;
					requestBuilder.Fragment = null;
					observedRequests.Add(requestBuilder.Uri.AbsoluteUri);
				}

				Touch();
			}
		}

		/// <summary>
		/// Called by every internal/public method on this class to give
		/// periodic operations a chance to run.
		/// </summary>
		protected static void Touch() {
			// Publish stats if it's time to do so.
			lock (publishingConsiderationLock) {
				if (DateTime.Now - lastPublished > Configuration.MinimumReportingInterval) {
					lastPublished = DateTime.Now;
					var fireAndForget = SendStatsAsync();
				}
			}
		}

		/// <summary>
		/// Initializes Reporting if it has not been initialized yet.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method must never throw.")]
		private static void Initialize() {
			lock (initializationSync) {
				if (!broken && !initialized) {
					try {
						file = GetIsolatedStorage();
						reportOriginIdentity = GetOrCreateOriginIdentity();

						observations.Add(observedRequests = new PersistentHashSet(file, "requests.txt", 3));
						observations.Add(observedCultures = new PersistentHashSet(file, "cultures.txt", 20));
						observations.Add(observedFeatures = new PersistentHashSet(file, "features.txt", int.MaxValue));

						// Record site-wide features in use.
						if (HttpContext.Current != null && HttpContext.Current.ApplicationInstance != null) {
							// MVC or web forms?
							// front-end or back end web farm?
							// url rewriting?
							////RecordFeatureUse(IsMVC ? "ASP.NET MVC" : "ASP.NET Web Forms");
						}

						initialized = true;
					} catch (Exception e) {
						// This is supposed to be as low-risk as possible, so if it fails, just disable reporting
						// and avoid rethrowing.
						broken = true;
						Logger.Library.ErrorException("Error while trying to initialize reporting.", e);
					}
				}
			}
		}

		/// <summary>
		/// Creates an HTTP client that can be used for outbound HTTP requests.
		/// </summary>
		/// <returns>The HTTP client to use.</returns>
		private static HttpClient CreateHttpClient() {
			var channel = new HttpClientHandler();
			channel.AllowAutoRedirect = false;
			var webRequestHandler = new HttpClient(channel);
			webRequestHandler.DefaultRequestHeaders.UserAgent.Add(Util.LibraryVersionHeader);
			return webRequestHandler;
		}

		/// <summary>
		/// Assembles a report for submission.
		/// </summary>
		/// <returns>A stream that contains the report.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "If we dispose of the StreamWriter, it disposes of the underlying stream.")]
		private static Stream GetReport() {
			var stream = new MemoryStream();
			try {
				var writer = new StreamWriter(stream, Encoding.UTF8);
				writer.WriteLine(reportOriginIdentity.ToString("B"));
				writer.WriteLine(Util.LibraryVersion);
				writer.WriteLine(".NET Framework {0}", Environment.Version);

				foreach (var observation in observations) {
					observation.Flush();
					writer.WriteLine("====================================");
					writer.WriteLine(observation.FileName);
					try {
						using (var fileStream = new IsolatedStorageFileStream(observation.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, file)) {
							writer.Flush();
							fileStream.CopyTo(writer.BaseStream);
						}
					} catch (FileNotFoundException) {
						writer.WriteLine("(missing)");
					}
				}

				// Not all event counters may have even loaded in this app instance.
				// We flush the ones in memory, and then read all of them off disk.
				foreach (var counter in events.Values) {
					counter.Flush();
				}

				foreach (string eventFile in file.GetFileNames("event-*.txt")) {
					writer.WriteLine("====================================");
					writer.WriteLine(eventFile);
					using (var fileStream = new IsolatedStorageFileStream(eventFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, file)) {
						writer.Flush();
						fileStream.CopyTo(writer.BaseStream);
					}
				}

				// Make sure the stream is positioned at the beginning.
				writer.Flush();
				stream.Position = 0;
				return stream;
			} catch {
				stream.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Sends the usage reports to the library authors.
		/// </summary>
		/// <returns>A value indicating whether submitting the report was successful.</returns>
		private static async Task<bool> SendStatsAsync() {
			try {
				Stream report = GetReport();
				var content = new StreamContent(report);
				content.Headers.ContentType = new MediaTypeHeaderValue("text/dnoa-report1");
				using (var webRequestHandler = CreateHttpClient()) {
					using (var response = await webRequestHandler.PostAsync(wellKnownPostLocation, content)) {
						Logger.Library.Info("Statistical report submitted successfully.");

						// The response stream may contain a message for the webmaster.
						// Since as part of the report we submit the library version number,
						// the report receiving service may have alerts such as:
						// "You're using an obsolete version with exploitable security vulnerabilities."
						using (var responseReader = new StreamReader(await response.Content.ReadAsStreamAsync())) {
							string line = await responseReader.ReadLineAsync();
							if (line != null) {
								DemuxLogMessage(line);
							}
						}
					}
				}

				// Report submission was successful.  Reset all counters.
				lock (events) {
					foreach (PersistentCounter counter in events.Values) {
						counter.Reset();
						counter.Flush();
					}

					// We can just delete the files for counters that are not currently loaded.
					foreach (string eventFile in file.GetFileNames("event-*.txt")) {
						if (!events.Values.Any(e => string.Equals(e.FileName, eventFile, StringComparison.OrdinalIgnoreCase))) {
							file.DeleteFile(eventFile);
						}
					}
				}

				return true;
			} catch (ProtocolException ex) {
				Logger.Library.ErrorException("Unable to submit statistical report due to an HTTP error.", ex);
			} catch (FileNotFoundException ex) {
                Logger.Library.ErrorException("Unable to submit statistical report because the report file is missing.", ex);
			}

			return false;
		}

		/// <summary>
		/// Interprets the reporting response as a log message if possible.
		/// </summary>
		/// <param name="line">The line from the HTTP response to interpret as a log message.</param>
		private static void DemuxLogMessage(string line) {
			if (line != null) {
				string[] parts = line.Split(new char[] { ' ' }, 2);
				if (parts.Length == 2) {
					string level = parts[0];
					string message = parts[1];
					switch (level) {
						case "INFO":
							Logger.Library.Info(message);
							break;
						case "WARN":
							Logger.Library.Warn(message);
							break;
						case "ERROR":
							Logger.Library.Error(message);
							break;
						case "FATAL":
							Logger.Library.Fatal(message);
							break;
					}
				}
			}
		}

		/// <summary>
		/// Gets the isolated storage to use for reporting.
		/// </summary>
		/// <returns>An isolated storage location appropriate for our host.</returns>
		private static IsolatedStorageFile GetIsolatedStorage() {
			IsolatedStorageFile result = null;

			// We'll try for whatever storage location we can get,
			// and not catch exceptions from the last attempt so that
			// the overall failure is caught by our caller.
			try {
				// This works on Personal Web Server
				result = IsolatedStorageFile.GetUserStoreForDomain();
			} catch (SecurityException) {
			} catch (IsolatedStorageException) {
			}

			// This works on IIS when full trust is granted.
			if (result == null) {
				result = IsolatedStorageFile.GetMachineStoreForDomain();
			}

			Logger.Library.InfoFormat("Reporting will use isolated storage with scope: {0}", result.Scope);
			return result;
		}

		/// <summary>
		/// Gets a unique, pseudonymous identifier for this particular web site or application.  
		/// </summary>
		/// <returns>A GUID that will serve as the identifier.</returns>
		/// <remarks>
		/// The identifier is made persistent by storing the identifier in isolated storage.
		/// If an existing identifier is not found, a new one is created, persisted, and returned.
		/// </remarks>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "No apparent problem.  False positive?")]
		private static Guid GetOrCreateOriginIdentity() {
			RequiresEx.ValidState(file != null, "file not set.");

			Guid identityGuid = Guid.Empty;
			const int GuidLength = 16;
			using (var identityFileStream = new IsolatedStorageFileStream("identity.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, file)) {
				if (identityFileStream.Length == GuidLength) {
					byte[] guidBytes = new byte[GuidLength];
					if (identityFileStream.Read(guidBytes, 0, GuidLength) == GuidLength) {
						identityGuid = new Guid(guidBytes);
					}
				}

				if (identityGuid == Guid.Empty) {
					identityGuid = Guid.NewGuid();
					byte[] guidBytes = identityGuid.ToByteArray();
					identityFileStream.SetLength(0);
					identityFileStream.Write(guidBytes, 0, guidBytes.Length);
				}

				return identityGuid;
			}
		}

		/// <summary>
		/// Sanitizes the name of the file so it only includes valid filename characters.
		/// </summary>
		/// <param name="fileName">The filename to sanitize.</param>
		/// <returns>The filename, with any and all invalid filename characters replaced with the hyphen (-) character.</returns>
		private static string SanitizeFileName(string fileName) {
			Requires.NotNullOrEmpty(fileName, "fileName");
			char[] invalidCharacters = Path.GetInvalidFileNameChars();
			if (fileName.IndexOfAny(invalidCharacters) < 0) {
				return fileName; // nothing invalid about this filename.
			}

			// Use a stringbuilder since we may be replacing several characters
			// and we don't want to instantiate a new string buffer for each new version.
			StringBuilder sanitized = new StringBuilder(fileName);
			foreach (char invalidChar in invalidCharacters) {
				sanitized.Replace(invalidChar, '-');
			}

			return sanitized.ToString();
		}

		/// <summary>
		/// A set of values that persist the set to disk.
		/// </summary>
		internal class PersistentHashSet : IDisposable {
			/// <summary>
			/// The isolated persistent storage.
			/// </summary>
			private readonly FileStream fileStream;

			/// <summary>
			/// The persistent reader.
			/// </summary>
			private readonly StreamReader reader;

			/// <summary>
			/// The persistent writer.
			/// </summary>
			private readonly StreamWriter writer;

			/// <summary>
			/// The total set of elements.
			/// </summary>
			private readonly HashSet<string> memorySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			/// <summary>
			/// The maximum number of elements to track before not storing new elements.
			/// </summary>
			private readonly int maximumElements;

			/// <summary>
			/// The set of new elements added to the <see cref="memorySet"/> since the last flush.
			/// </summary>
			private List<string> newElements = new List<string>();

			/// <summary>
			/// The time the last flush occurred.
			/// </summary>
			private DateTime lastFlushed;

			/// <summary>
			/// A flag indicating whether the set has changed since it was last flushed.
			/// </summary>
			private bool dirty;

			/// <summary>
			/// Initializes a new instance of the <see cref="PersistentHashSet"/> class.
			/// </summary>
			/// <param name="storage">The storage location.</param>
			/// <param name="fileName">Name of the file.</param>
			/// <param name="maximumElements">The maximum number of elements to track.</param>
			internal PersistentHashSet(IsolatedStorageFile storage, string fileName, int maximumElements) {
				Requires.NotNull(storage, "storage");
				Requires.NotNullOrEmpty(fileName, "fileName");
				this.FileName = fileName;
				this.maximumElements = maximumElements;

				// Load the file into memory.
				this.fileStream = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, storage);
				this.reader = new StreamReader(this.fileStream, Encoding.UTF8);
				while (!this.reader.EndOfStream) {
					this.memorySet.Add(this.reader.ReadLine());
				}

				this.writer = new StreamWriter(this.fileStream, Utf8NoPreamble);
				this.lastFlushed = DateTime.Now;
			}

			/// <summary>
			/// Gets a value indicating whether the hashset has reached capacity and is not storing more elements.
			/// </summary>
			/// <value><c>true</c> if this instance is full; otherwise, <c>false</c>.</value>
			internal bool IsFull {
				get {
					lock (this.memorySet) {
						return this.memorySet.Count >= this.maximumElements;
					}
				}
			}

			/// <summary>
			/// Gets the name of the file.
			/// </summary>
			/// <value>The name of the file.</value>
			internal string FileName { get; private set; }

			#region IDisposable Members

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose() {
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			#endregion

			/// <summary>
			/// Adds a value to the set.
			/// </summary>
			/// <param name="value">The value.</param>
			internal void Add(string value) {
				lock (this.memorySet) {
					if (!this.IsFull) {
						if (this.memorySet.Add(value)) {
							this.newElements.Add(value);
							this.dirty = true;

							if (this.IsFull) {
								this.Flush();
							}
						}

						if (this.dirty && DateTime.Now - this.lastFlushed > Configuration.MinimumFlushInterval) {
							this.Flush();
						}
					}
				}
			}

			/// <summary>
			/// Flushes any newly added values to disk.
			/// </summary>
			internal void Flush() {
				lock (this.memorySet) {
					foreach (string element in this.newElements) {
						this.writer.WriteLine(element);
					}
					this.writer.Flush();

					// Assign a whole new list since future lists might be smaller in order to
					// decrease demand on memory.
					this.newElements = new List<string>();
					this.dirty = false;
					this.lastFlushed = DateTime.Now;
				}
			}

			/// <summary>
			/// Releases unmanaged and - optionally - managed resources
			/// </summary>
			/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
			protected virtual void Dispose(bool disposing) {
				if (disposing) {
					this.writer.Dispose();
					this.reader.Dispose();
					this.fileStream.Dispose();
				}
			}
		}

		/// <summary>
		/// A feature usage counter.
		/// </summary>
		private class PersistentCounter : IDisposable {
			/// <summary>
			/// The separator to use between category names and their individual counters.
			/// </summary>
			private static readonly char[] separator = new char[] { '\t' };

			/// <summary>
			/// The isolated persistent storage.
			/// </summary>
			private readonly FileStream fileStream;

			/// <summary>
			/// The persistent reader.
			/// </summary>
			private readonly StreamReader reader;

			/// <summary>
			/// The persistent writer.
			/// </summary>
			private readonly StreamWriter writer;

			/// <summary>
			/// The time the last flush occurred.
			/// </summary>
			private DateTime lastFlushed;

			/// <summary>
			/// The in-memory copy of the counter.
			/// </summary>
			private Dictionary<string, int> counters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			/// <summary>
			/// A flag indicating whether the set has changed since it was last flushed.
			/// </summary>
			private bool dirty;

			/// <summary>
			/// Initializes a new instance of the <see cref="PersistentCounter"/> class.
			/// </summary>
			/// <param name="storage">The storage location.</param>
			/// <param name="fileName">Name of the file.</param>
			internal PersistentCounter(IsolatedStorageFile storage, string fileName) {
				Requires.NotNull(storage, "storage");
				Requires.NotNullOrEmpty(fileName, "fileName");
				this.FileName = fileName;

				// Load the file into memory.
				this.fileStream = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, storage);
				this.reader = new StreamReader(this.fileStream, Encoding.UTF8);
				while (!this.reader.EndOfStream) {
					string line = this.reader.ReadLine();
					string[] parts = line.Split(separator, 2);
					int counter;
					if (int.TryParse(parts[0], out counter)) {
						string category = string.Empty;
						if (parts.Length > 1) {
							category = parts[1];
						}
						this.counters[category] = counter;
					}
				}

				this.writer = new StreamWriter(this.fileStream, Utf8NoPreamble);
				this.lastFlushed = DateTime.Now;
			}

			/// <summary>
			/// Gets the name of the file.
			/// </summary>
			/// <value>The name of the file.</value>
			internal string FileName { get; private set; }

			#region IDisposable Members

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose() {
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			#endregion

			/// <summary>
			/// Increments the counter.
			/// </summary>
			/// <param name="category">The category within the event.  Null and empty strings are allowed, but considered the same.</param>
			internal void Increment(string category) {
				if (category == null) {
					category = string.Empty;
				}
				lock (this) {
					int counter;
					this.counters.TryGetValue(category, out counter);
					this.counters[category] = counter + 1;
					this.dirty = true;
					if (this.dirty && DateTime.Now - this.lastFlushed > Configuration.MinimumFlushInterval) {
						this.Flush();
					}
				}
			}

			/// <summary>
			/// Flushes any newly added values to disk.
			/// </summary>
			internal void Flush() {
				lock (this) {
					this.writer.BaseStream.Position = 0;
					this.writer.BaseStream.SetLength(0); // truncate file
					foreach (var pair in this.counters) {
						this.writer.Write(pair.Value);
						this.writer.Write(separator[0]);
						this.writer.WriteLine(pair.Key);
					}
					this.writer.Flush();
					this.dirty = false;
					this.lastFlushed = DateTime.Now;
				}
			}

			/// <summary>
			/// Resets all counters.
			/// </summary>
			internal void Reset() {
				lock (this) {
					this.counters.Clear();
				}
			}

			/// <summary>
			/// Releases unmanaged and - optionally - managed resources
			/// </summary>
			/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
			protected virtual void Dispose(bool disposing) {
				if (disposing) {
					this.writer.Dispose();
					this.reader.Dispose();
					this.fileStream.Dispose();
				}
			}
		}
	}
}
