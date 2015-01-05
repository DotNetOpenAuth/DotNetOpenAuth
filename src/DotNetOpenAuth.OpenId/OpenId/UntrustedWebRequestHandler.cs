//-----------------------------------------------------------------------
// <copyright file="UntrustedWebRequestHandler.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Net.Cache;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A paranoid HTTP get/post request engine.  It helps to protect against attacks from remote
	/// server leaving dangling connections, sending too much data, causing requests against 
	/// internal servers, etc.
	/// </summary>
	/// <remarks>
	/// Protections include:
	/// * Conservative maximum time to receive the complete response.
	/// * Only HTTP and HTTPS schemes are permitted.
	/// * Internal IP address ranges are not permitted: 127.*.*.*, 1::*
	/// * Internal host names are not permitted (periods must be found in the host name)
	/// If a particular host would be permitted but is in the blacklist, it is not allowed.
	/// If a particular host would not be permitted but is in the whitelist, it is allowed.
	/// </remarks>
	public class UntrustedWebRequestHandler : DelegatingHandler {
		/// <summary>
		/// The set of URI schemes allowed in untrusted web requests.
		/// </summary>
		private ICollection<string> allowableSchemes = new List<string> { "http", "https" };

		/// <summary>
		/// The collection of blacklisted hosts.
		/// </summary>
		private ICollection<string> blacklistHosts = new List<string>(Configuration.BlacklistHosts.KeysAsStrings);

		/// <summary>
		/// The collection of regular expressions used to identify additional blacklisted hosts.
		/// </summary>
		private ICollection<Regex> blacklistHostsRegex = new List<Regex>(Configuration.BlacklistHostsRegex.KeysAsRegexs);

		/// <summary>
		/// The collection of whitelisted hosts.
		/// </summary>
		private ICollection<string> whitelistHosts = new List<string>(Configuration.WhitelistHosts.KeysAsStrings);

		/// <summary>
		/// The collection of regular expressions used to identify additional whitelisted hosts.
		/// </summary>
		private ICollection<Regex> whitelistHostsRegex = new List<Regex>(Configuration.WhitelistHostsRegex.KeysAsRegexs);

		/// <summary>
		/// The maximum redirections to follow in the course of a single request.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int maxAutomaticRedirections = Configuration.MaximumRedirections;

		/// <summary>
		/// A value indicating whether to automatically follow redirects.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool allowAutoRedirect = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="UntrustedWebRequestHandler" /> class.
		/// </summary>
		/// <param name="innerHandler">
		/// The inner handler. This handler will be modified to suit the purposes of this wrapping handler,
		/// and should not be used independently of this wrapper after construction of this object.
		/// </param>
		public UntrustedWebRequestHandler(WebRequestHandler innerHandler = null)
			: base(innerHandler ?? new WebRequestHandler()) {
			// If SSL is required throughout, we cannot allow auto redirects because
			// it may include a pass through an unprotected HTTP request.
			// We have to follow redirects manually.
			// It also allows us to ignore HttpWebResponse.FinalUri since that can be affected by
			// the Content-Location header and open security holes.
			this.MaxAutomaticRedirections = Configuration.MaximumRedirections;
			this.InnerWebRequestHandler.AllowAutoRedirect = false;

			if (Debugger.IsAttached) {
				// Since a debugger is attached, requests may be MUCH slower,
				// so give ourselves huge timeouts.
				this.InnerWebRequestHandler.ReadWriteTimeout = (int)TimeSpan.FromHours(1).TotalMilliseconds;
			} else {
				this.InnerWebRequestHandler.ReadWriteTimeout = (int)Configuration.ReadWriteTimeout.TotalMilliseconds;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UntrustedWebRequestHandler"/> class
		/// for use in unit testing.
		/// </summary>
		/// <param name="innerHandler">
		/// The inner handler which is responsible for processing the HTTP response messages.
		/// This handler should NOT automatically follow redirects.
		/// </param>
		internal UntrustedWebRequestHandler(HttpMessageHandler innerHandler)
			: base(innerHandler) {
		}

		/// <summary>
		/// Gets or sets a value indicating whether all requests must use SSL.
		/// </summary>
		/// <value>
		/// <c>true</c> if SSL is required; otherwise, <c>false</c>.
		/// </value>
		public bool IsSslRequired { get; set; }

		/// <summary>
		/// Gets or sets the total number of redirections to allow on any one request.
		/// Default is 10.
		/// </summary>
		public int MaxAutomaticRedirections {
			get {
				return this.InnerHandler is WebRequestHandler ? this.InnerWebRequestHandler.MaxAutomaticRedirections : this.maxAutomaticRedirections;
			}

			set {
				Requires.Range(value >= 0, "value");
				this.maxAutomaticRedirections = value;
				if (this.InnerHandler is WebRequestHandler) {
					this.InnerWebRequestHandler.MaxAutomaticRedirections = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to automatically follow redirects.
		/// </summary>
		public bool AllowAutoRedirect {
			get {
				return this.InnerHandler is WebRequestHandler ? this.InnerWebRequestHandler.AllowAutoRedirect : this.allowAutoRedirect;
			}

			set {
				this.allowAutoRedirect = value;
				if (this.InnerHandler is WebRequestHandler) {
					this.InnerWebRequestHandler.AllowAutoRedirect = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the time (in milliseconds) allowed to wait for single read or write operation to complete.
		/// Default is 500 milliseconds.
		/// </summary>
		public int ReadWriteTimeout {
			get { return this.InnerWebRequestHandler.ReadWriteTimeout; }
			set { this.InnerWebRequestHandler.ReadWriteTimeout = value; }
		}

		/// <summary>
		/// Gets a collection of host name literals that should be allowed even if they don't
		/// pass standard security checks.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Whitelist",
			Justification = "Spelling as intended.")]
		public ICollection<string> WhitelistHosts {
			get {
				return this.whitelistHosts;
			}
		}

		/// <summary>
		/// Gets a collection of host name regular expressions that indicate hosts that should
		/// be allowed even though they don't pass standard security checks.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Whitelist",
			Justification = "Spelling as intended.")]
		public ICollection<Regex> WhitelistHostsRegex {
			get {
				return this.whitelistHostsRegex;
			}
		}

		/// <summary>
		/// Gets a collection of host name literals that should be rejected even if they 
		/// pass standard security checks.
		/// </summary>
		public ICollection<string> BlacklistHosts {
			get {
				return this.blacklistHosts;
			}
		}

		/// <summary>
		/// Gets a collection of host name regular expressions that indicate hosts that should
		/// be rejected even if they pass standard security checks.
		/// </summary>
		public ICollection<Regex> BlacklistHostsRegex {
			get {
				return this.blacklistHostsRegex;
			}
		}

		/// <summary>
		/// Gets the inner web request handler.
		/// </summary>
		/// <value>
		/// The inner web request handler.
		/// </value>
		public WebRequestHandler InnerWebRequestHandler {
			get { return (WebRequestHandler)this.InnerHandler; }
		}

		/// <summary>
		/// Gets the configuration for this class that is specified in the host's .config file.
		/// </summary>
		private static UntrustedWebRequestElement Configuration {
			get { return DotNetOpenAuthSection.Messaging.UntrustedWebRequest; }
		}

		/// <summary>
		/// Creates an HTTP client that uses this instance as an HTTP handler.
		/// </summary>
		/// <returns>The initialized instance.</returns>
		public HttpClient CreateClient() {
			var client = new HttpClient(this);
			client.MaxResponseContentBufferSize = Configuration.MaximumBytesToRead;

			if (Debugger.IsAttached) {
				// Since a debugger is attached, requests may be MUCH slower,
				// so give ourselves huge timeouts.
				client.Timeout = TimeSpan.FromHours(1);
			} else {
				client.Timeout = Configuration.Timeout;
			}

			return client;
		}

		/// <summary>
		/// Determines whether an exception was thrown because of the remote HTTP server returning HTTP 417 Expectation Failed.
		/// </summary>
		/// <param name="ex">The caught exception.</param>
		/// <returns>
		/// 	<c>true</c> if the failure was originally caused by a 417 Exceptation Failed error; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsExceptionFrom417ExpectationFailed(Exception ex) {
			while (ex != null) {
				WebException webEx = ex as WebException;
				if (webEx != null) {
					HttpWebResponse response = webEx.Response as HttpWebResponse;
					if (response != null) {
						if (response.StatusCode == HttpStatusCode.ExpectationFailed) {
							return true;
						}
					}
				}

				ex = ex.InnerException;
			}

			return false;
		}

		/// <summary>
		/// Send an HTTP request as an asynchronous operation.
		/// </summary>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="cancellationToken">The cancellation token to cancel operation.</param>
		/// <returns>
		/// Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.
		/// </returns>
		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request, CancellationToken cancellationToken) {
			this.EnsureAllowableRequestUri(request.RequestUri);

			// Since we may require SSL for every redirect, we handle each redirect manually
			// in order to detect and fail if any redirect sends us to an HTTP url.
			// We COULD allow automatic redirect in the cases where HTTPS is not required,
			// but our mock request infrastructure can't do redirects on its own either.
			Uri originalRequestUri = request.RequestUri;
			int i;
			for (i = 0; i < this.MaxAutomaticRedirections; i++) {
				this.EnsureAllowableRequestUri(request.RequestUri);
				var response = await base.SendAsync(request, cancellationToken);
				if (this.AllowAutoRedirect) {
					if (response.StatusCode == HttpStatusCode.MovedPermanently || response.StatusCode == HttpStatusCode.Redirect
						|| response.StatusCode == HttpStatusCode.RedirectMethod
						|| response.StatusCode == HttpStatusCode.RedirectKeepVerb) {
						// We have no copy of the post entity stream to repeat on our manually
						// cloned HttpWebRequest, so we have to bail.
						ErrorUtilities.VerifyProtocol(
							request.Method != HttpMethod.Post, MessagingStrings.UntrustedRedirectsOnPOSTNotSupported);
						Uri redirectUri = new Uri(request.RequestUri, response.Headers.Location);
						request = request.Clone();
						request.RequestUri = redirectUri;
						continue;
					}
				}

				if (response.StatusCode == HttpStatusCode.ExpectationFailed) {
					// Some OpenID servers doesn't understand the Expect header and send 417 error back.
					// If this server just failed from that, alter the ServicePoint for this server
					// so that we don't send that header again next time (whenever that is).
					// "Expect: 100-Continue" HTTP header. (see Google Code Issue 72)
					// We don't want to blindly set all ServicePoints to not use the Expect header
					// as that would be a security hole allowing any visitor to a web site change
					// the web site's global behavior when calling that host.
					// TODO 5.0: verify that this still works in DNOA 5.0
					var servicePoint = ServicePointManager.FindServicePoint(request.RequestUri);
					Logger.Http.InfoFormat(
						"HTTP POST to {0} resulted in 417 Expectation Failed.  Changing ServicePoint to not use Expect: Continue next time.",
						request.RequestUri);
					servicePoint.Expect100Continue = false;
				}

				return response;
			}

			throw ErrorUtilities.ThrowProtocol(MessagingStrings.TooManyRedirects, originalRequestUri);
		}

		/// <summary>
		/// Determines whether an IP address is the IPv6 equivalent of "localhost/127.0.0.1".
		/// </summary>
		/// <param name="ip">The ip address to check.</param>
		/// <returns>
		/// 	<c>true</c> if this is a loopback IP address; <c>false</c> otherwise.
		/// </returns>
		private static bool IsIPv6Loopback(IPAddress ip) {
			Requires.NotNull(ip, "ip");
			byte[] addressBytes = ip.GetAddressBytes();
			for (int i = 0; i < addressBytes.Length - 1; i++) {
				if (addressBytes[i] != 0) {
					return false;
				}
			}
			if (addressBytes[addressBytes.Length - 1] != 1) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// Determines whether the given host name is in a host list or host name regex list.
		/// </summary>
		/// <param name="host">The host name.</param>
		/// <param name="stringList">The list of host names.</param>
		/// <param name="regexList">The list of regex patterns of host names.</param>
		/// <returns>
		/// 	<c>true</c> if the specified host falls within at least one of the given lists; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsHostInList(string host, ICollection<string> stringList, ICollection<Regex> regexList) {
			Requires.NotNullOrEmpty(host, "host");
			Requires.NotNull(stringList, "stringList");
			Requires.NotNull(regexList, "regexList");
			foreach (string testHost in stringList) {
				if (string.Equals(host, testHost, StringComparison.OrdinalIgnoreCase)) {
					return true;
				}
			}
			foreach (Regex regex in regexList) {
				if (regex.IsMatch(host)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether a given host is whitelisted.
		/// </summary>
		/// <param name="host">The host name to test.</param>
		/// <returns>
		/// 	<c>true</c> if the host is whitelisted; otherwise, <c>false</c>.
		/// </returns>
		private bool IsHostWhitelisted(string host) {
			return IsHostInList(host, this.WhitelistHosts, this.WhitelistHostsRegex);
		}

		/// <summary>
		/// Determines whether a given host is blacklisted.
		/// </summary>
		/// <param name="host">The host name to test.</param>
		/// <returns>
		/// 	<c>true</c> if the host is blacklisted; otherwise, <c>false</c>.
		/// </returns>
		private bool IsHostBlacklisted(string host) {
			return IsHostInList(host, this.BlacklistHosts, this.BlacklistHostsRegex);
		}

		/// <summary>
		/// Verify that the request qualifies under our security policies
		/// </summary>
		/// <param name="requestUri">The request URI.</param>
		/// <exception cref="ProtocolException">Thrown when the URI is disallowed for security reasons.</exception>
		private void EnsureAllowableRequestUri(Uri requestUri) {
			ErrorUtilities.VerifyProtocol(
				this.IsUriAllowable(requestUri), MessagingStrings.UnsafeWebRequestDetected, requestUri);
			ErrorUtilities.VerifyProtocol(
				!this.IsSslRequired || string.Equals(requestUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase),
				MessagingStrings.InsecureWebRequestWithSslRequired,
				requestUri);
		}

		/// <summary>
		/// Determines whether a URI is allowed based on scheme and host name.
		/// No requireSSL check is done here
		/// </summary>
		/// <param name="uri">The URI to test for whether it should be allowed.</param>
		/// <returns>
		/// 	<c>true</c> if [is URI allowable] [the specified URI]; otherwise, <c>false</c>.
		/// </returns>
		private bool IsUriAllowable(Uri uri) {
			Requires.NotNull(uri, "uri");
			if (!this.allowableSchemes.Contains(uri.Scheme)) {
				Logger.Http.WarnFormat("Rejecting URL {0} because it uses a disallowed scheme.", uri);
				return false;
			}

			// Allow for whitelist or blacklist to override our detection.
			Func<string, bool> failsUnlessWhitelisted = (string reason) => {
				if (IsHostWhitelisted(uri.DnsSafeHost)) {
					return true;
				}
				Logger.Http.WarnFormat("Rejecting URL {0} because {1}.", uri, reason);
				return false;
			};

			// Try to interpret the hostname as an IP address so we can test for internal
			// IP address ranges.  Note that IP addresses can appear in many forms 
			// (e.g. http://127.0.0.1, http://2130706433, http://0x0100007f, http://::1
			// So we convert them to a canonical IPAddress instance, and test for all
			// non-routable IP ranges: 10.*.*.*, 127.*.*.*, ::1
			// Note that Uri.IsLoopback is very unreliable, not catching many of these variants.
			IPAddress hostIPAddress;
			if (IPAddress.TryParse(uri.DnsSafeHost, out hostIPAddress)) {
				byte[] addressBytes = hostIPAddress.GetAddressBytes();

				// The host is actually an IP address.
				switch (hostIPAddress.AddressFamily) {
					case System.Net.Sockets.AddressFamily.InterNetwork:
						if (addressBytes[0] == 127 || addressBytes[0] == 10) {
							return failsUnlessWhitelisted("it is a loopback address.");
						}
						break;
					case System.Net.Sockets.AddressFamily.InterNetworkV6:
						if (IsIPv6Loopback(hostIPAddress)) {
							return failsUnlessWhitelisted("it is a loopback address.");
						}
						break;
					default:
						return failsUnlessWhitelisted("it does not use an IPv4 or IPv6 address.");
				}
			} else {
				// The host is given by name.  We require names to contain periods to
				// help make sure it's not an internal address.
				if (!uri.Host.Contains(".")) {
					return failsUnlessWhitelisted("it does not contain a period in the host name.");
				}
			}
			if (this.IsHostBlacklisted(uri.DnsSafeHost)) {
				Logger.Http.WarnFormat("Rejected URL {0} because it is blacklisted.", uri);
				return false;
			}
			return true;
		}
	}
}
