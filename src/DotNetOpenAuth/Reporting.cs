//-----------------------------------------------------------------------
// <copyright file="Reporting.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.IO.IsolatedStorage;
	using System.Linq;
	using System.Net;
	using System.Reflection;
	using System.Security;
	using System.Text;
	using System.Threading;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// The statistical reporting mechanism used so this library's project authors
	/// know what versions and features are in use.
	/// </summary>
	internal static class Reporting {
		/// <summary>
		/// The maximum frequency that reports will be published.
		/// </summary>
		private static readonly TimeSpan minimumReportingInterval = TimeSpan.FromDays(1);

		/// <summary>
		/// The isolated storage to use for collecting data in between published reports.
		/// </summary>
		private static IsolatedStorageFile file;

		/// <summary>
		/// The name of this assembly.
		/// </summary>
		private static AssemblyName assemblyName;

		/// <summary>
		/// The recipient of collected reports.
		/// </summary>
		private static Uri wellKnownPostLocation = new Uri("http://reports.dotnetopenauth.net/ReportingPost.aspx");

		/// <summary>
		/// The outgoing HTTP request handler to use for publishing reports.
		/// </summary>
		private static IDirectWebRequestHandler webRequestHandler;

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
		static Reporting() {
			Enabled = DotNetOpenAuthSection.Configuration.Reporting.Enabled;
			if (Enabled) {
				try {
					file = GetIsolatedStorage();
					assemblyName = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
					webRequestHandler = new StandardWebRequestHandler();
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
				} catch (Exception e) {
					// This is supposed to be as low-risk as possible, so if it fails, just disable reporting
					// and avoid rethrowing.
					Enabled = false;
					Logger.Library.Error("Error while trying to initialize reporting.", e);
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this reporting is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		private static bool Enabled { get; set; }

		/// <summary>
		/// Records the use of a feature by name.
		/// </summary>
		/// <param name="feature">The feature.</param>
		internal static void RecordFeatureUse(string feature) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(feature));

			if (Enabled) {
				observedFeatures.Add(feature);
				Touch();
			}
		}

		/// <summary>
		/// Records the use of a feature by object type.
		/// </summary>
		/// <param name="value">The object whose type is the feature to set as used.</param>
		internal static void RecordFeatureUse(object value) {
			Contract.Requires<ArgumentNullException>(value != null);

			if (Enabled) {
				observedFeatures.Add(value.GetType().Name);
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
			Contract.Requires<ArgumentNullException>(value != null);

			if (Enabled) {
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
		/// Records the feature and dependency use.
		/// </summary>
		/// <param name="value">The consumer or service provider.</param>
		/// <param name="service">The service.</param>
		/// <param name="tokenManager">The token manager.</param>
		/// <param name="nonceStore">The nonce store.</param>
		internal static void RecordFeatureAndDependencyUse(object value, ServiceProviderDescription service, ITokenManager tokenManager, INonceStore nonceStore) {
			Contract.Requires<ArgumentNullException>(value != null);
			Contract.Requires<ArgumentNullException>(service != null);
			Contract.Requires<ArgumentNullException>(tokenManager != null);

			if (Enabled) {
				StringBuilder builder = new StringBuilder();
				builder.Append(value.GetType().Name);
				builder.Append(" ");
				builder.Append(tokenManager.GetType().Name);
				if (nonceStore != null) {
					builder.Append(" ");
					builder.Append(nonceStore.GetType().Name);
				}
				builder.Append(" ");
				builder.Append(service.Version);
				builder.Append(" ");
				builder.Append(service.UserAuthorizationEndpoint);
				observedFeatures.Add(builder.ToString());
				Touch();
			}
		}

		/// <summary>
		/// Records statistics collected from incoming requests.
		/// </summary>
		/// <param name="request">The request.</param>
		internal static void RecordRequestStatistics(HttpRequestInfo request) {
			Contract.Requires<ArgumentNullException>(request != null);

			if (Enabled) {
				observedCultures.Add(Thread.CurrentThread.CurrentCulture.Name);

				if (!observedRequests.IsFull) {
					var requestBuilder = new UriBuilder(request.UrlBeforeRewriting);
					requestBuilder.Query = null;
					requestBuilder.Fragment = null;
					observedRequests.Add(requestBuilder.Uri.AbsoluteUri);
				}

				Touch();
			}
		}

		/// <summary>
		/// Assembles a report for submission.
		/// </summary>
		/// <returns>A stream that contains the report.</returns>
		private static Stream GetReport() {
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream, Encoding.UTF8);
			writer.WriteLine(Util.LibraryVersion);

			foreach (var observation in observations) {
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

			// Make sure the stream is positioned at the beginning.
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		/// <summary>
		/// Sends the usage reports to the library authors.
		/// </summary>
		/// <returns>A value indicating whether submitting the report was successful.</returns>
		private static bool SendStats() {
			try {
				var request = (HttpWebRequest)WebRequest.Create(wellKnownPostLocation);
				request.UserAgent = Util.LibraryVersion;
				request.AllowAutoRedirect = false;
				request.Method = "POST";
				var report = GetReport();
				request.ContentLength = report.Length;
				using (var requestStream = webRequestHandler.GetRequestStream(request)) {
					report.CopyTo(requestStream);
				}

				var response = webRequestHandler.GetResponse(request);
				response.Dispose();
				return true;
			} catch (ProtocolException ex) {
				Logger.Library.Error("Unable to submit report due to an HTTP error.", ex);
			} catch (FileNotFoundException ex) {
				Logger.Library.Error("Unable to submit report because the report file is missing.", ex);
			}

			return false;
		}

		/// <summary>
		/// Called by every internal/public method on this class to give
		/// periodic operations a chance to run.
		/// </summary>
		private static void Touch() {
			// Publish stats if it's time to do so.
			lock (publishingConsiderationLock) {
				if (DateTime.Now - lastPublished > minimumReportingInterval) {
					lastPublished = DateTime.Now;

					// Do it on a background thread since it could take a while and we
					// don't want to slow down this request we're borrowing.
					ThreadPool.QueueUserWorkItem(state => SendStats());
				}
			}
		}

		/// <summary>
		/// Gets the isolated storage to use for reporting.
		/// </summary>
		/// <returns>An isolated storage location appropriate for our host.</returns>
		private static IsolatedStorageFile GetIsolatedStorage() {
			Contract.Ensures(Contract.Result<IsolatedStorageFile>() != null);

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
		/// A set of values that persist the set to disk.
		/// </summary>
		private class PersistentHashSet : IDisposable {
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
			/// The maximum frequency the set can be flushed to disk.
			/// </summary>
#if DEBUG
			private static readonly TimeSpan minimumFlushInterval = TimeSpan.Zero;
#else
			private static readonly TimeSpan minimumFlushInterval = TimeSpan.FromMinutes(15);
#endif

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
				Contract.Requires<ArgumentNullException>(storage != null);
				Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(fileName));
				this.FileName = fileName;
				this.maximumElements = maximumElements;

				// Load the file into memory.
				bool fileCreated = storage.GetFileNames(fileName).Length == 0;
				this.fileStream = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, storage);
				this.reader = new StreamReader(this.fileStream, Encoding.UTF8);
				while (!this.reader.EndOfStream) {
					this.memorySet.Add(this.reader.ReadLine());
				}

				this.writer = new StreamWriter(this.fileStream, Encoding.UTF8);
				this.lastFlushed = DateTime.Now;

				// Write a unique header to the file so the report collector can match duplicates.
				if (fileCreated) {
					this.writer.WriteLine(Guid.NewGuid());
				}
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

						if (this.dirty && DateTime.Now - this.lastFlushed > minimumFlushInterval) {
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
	}
}
