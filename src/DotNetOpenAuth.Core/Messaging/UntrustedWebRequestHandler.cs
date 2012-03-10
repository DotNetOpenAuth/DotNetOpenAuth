//-----------------------------------------------------------------------
// <copyright file="UntrustedWebRequestHandler.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Net.Cache;
	using System.Text.RegularExpressions;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;

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
	public class UntrustedWebRequestHandler : IDirectWebRequestHandler {
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
		private int maximumRedirections = Configuration.MaximumRedirections;

		/// <summary>
		/// The maximum number of bytes to read from the response of an untrusted server.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int maximumBytesToRead = Configuration.MaximumBytesToRead;

		/// <summary>
		/// The handler that will actually send the HTTP request and collect
		/// the response once the untrusted server gates have been satisfied.
		/// </summary>
		private IDirectWebRequestHandler chainedWebRequestHandler;

		/// <summary>
		/// Initializes a new instance of the <see cref="UntrustedWebRequestHandler"/> class.
		/// </summary>
		public UntrustedWebRequestHandler()
			: this(new StandardWebRequestHandler()) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UntrustedWebRequestHandler"/> class.
		/// </summary>
		/// <param name="chainedWebRequestHandler">The chained web request handler.</param>
		public UntrustedWebRequestHandler(IDirectWebRequestHandler chainedWebRequestHandler) {
			Requires.NotNull(chainedWebRequestHandler, "chainedWebRequestHandler");

			this.chainedWebRequestHandler = chainedWebRequestHandler;
			if (Debugger.IsAttached) {
				// Since a debugger is attached, requests may be MUCH slower,
				// so give ourselves huge timeouts.
				this.ReadWriteTimeout = TimeSpan.FromHours(1);
				this.Timeout = TimeSpan.FromHours(1);
			} else {
				this.ReadWriteTimeout = Configuration.ReadWriteTimeout;
				this.Timeout = Configuration.Timeout;
			}
		}

		/// <summary>
		/// Gets or sets the default maximum bytes to read in any given HTTP request.
		/// </summary>
		/// <value>Default is 1MB.  Cannot be less than 2KB.</value>
		public int MaximumBytesToRead {
			get {
				return this.maximumBytesToRead;
			}

			set {
				Requires.InRange(value >= 2048, "value");
				this.maximumBytesToRead = value;
			}
		}

		/// <summary>
		/// Gets or sets the total number of redirections to allow on any one request.
		/// Default is 10.
		/// </summary>
		public int MaximumRedirections {
			get {
				return this.maximumRedirections;
			}

			set {
				Requires.InRange(value >= 0, "value");
				this.maximumRedirections = value;
			}
		}

		/// <summary>
		/// Gets or sets the time allowed to wait for single read or write operation to complete.
		/// Default is 500 milliseconds.
		/// </summary>
		public TimeSpan ReadWriteTimeout { get; set; }

		/// <summary>
		/// Gets or sets the time allowed for an entire HTTP request.  
		/// Default is 5 seconds.
		/// </summary>
		public TimeSpan Timeout { get; set; }

		/// <summary>
		/// Gets a collection of host name literals that should be allowed even if they don't
		/// pass standard security checks.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Whitelist", Justification = "Spelling as intended.")]
		public ICollection<string> WhitelistHosts { get { return this.whitelistHosts; } }

		/// <summary>
		/// Gets a collection of host name regular expressions that indicate hosts that should
		/// be allowed even though they don't pass standard security checks.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Whitelist", Justification = "Spelling as intended.")]
		public ICollection<Regex> WhitelistHostsRegex { get { return this.whitelistHostsRegex; } }

		/// <summary>
		/// Gets a collection of host name literals that should be rejected even if they 
		/// pass standard security checks.
		/// </summary>
		public ICollection<string> BlacklistHosts { get { return this.blacklistHosts; } }

		/// <summary>
		/// Gets a collection of host name regular expressions that indicate hosts that should
		/// be rejected even if they pass standard security checks.
		/// </summary>
		public ICollection<Regex> BlacklistHostsRegex { get { return this.blacklistHostsRegex; } }

		/// <summary>
		/// Gets the configuration for this class that is specified in the host's .config file.
		/// </summary>
		private static UntrustedWebRequestElement Configuration {
			get { return DotNetOpenAuthSection.Messaging.UntrustedWebRequest; }
		}

		#region IDirectWebRequestHandler Members

		/// <summary>
		/// Determines whether this instance can support the specified options.
		/// </summary>
		/// <param name="options">The set of options that might be given in a subsequent web request.</param>
		/// <returns>
		/// 	<c>true</c> if this instance can support the specified options; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		public bool CanSupport(DirectWebRequestOptions options) {
			// We support whatever our chained handler supports, plus RequireSsl.
			return this.chainedWebRequestHandler.CanSupport(options & ~DirectWebRequestOptions.RequireSsl);
		}

		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <param name="options">The options to apply to this web request.</param>
		/// <returns>
		/// The writer the caller should write out the entity data to.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>The caller should have set the <see cref="HttpWebRequest.ContentLength"/>
		/// and any other appropriate properties <i>before</i> calling this method.</para>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.</para>
		/// </remarks>
		public Stream GetRequestStream(HttpWebRequest request, DirectWebRequestOptions options) {
			this.EnsureAllowableRequestUri(request.RequestUri, (options & DirectWebRequestOptions.RequireSsl) != 0);

			this.PrepareRequest(request, true);

			// Submit the request and get the request stream back.
			return this.chainedWebRequestHandler.GetRequestStream(request, options & ~DirectWebRequestOptions.RequireSsl);
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the
		/// <see cref="HttpWebResponse"/> to a <see cref="IncomingWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <param name="options">The options to apply to this web request.</param>
		/// <returns>
		/// An instance of <see cref="CachedDirectWebResponse"/> describing the response.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.  The <see cref="WebException.Response"/>
		/// value, if set, should be Closed before throwing.</para>
		/// </remarks>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Uri(Uri, string) accepts second arguments that Uri(Uri, new Uri(string)) does not that we must support.")]
		public IncomingWebResponse GetResponse(HttpWebRequest request, DirectWebRequestOptions options) {
			// This request MAY have already been prepared by GetRequestStream, but
			// we have no guarantee, so do it just to be safe.
			this.PrepareRequest(request, false);

			// Since we may require SSL for every redirect, we handle each redirect manually
			// in order to detect and fail if any redirect sends us to an HTTP url.
			// We COULD allow automatic redirect in the cases where HTTPS is not required,
			// but our mock request infrastructure can't do redirects on its own either.
			Uri originalRequestUri = request.RequestUri;
			int i;
			for (i = 0; i < this.MaximumRedirections; i++) {
				this.EnsureAllowableRequestUri(request.RequestUri, (options & DirectWebRequestOptions.RequireSsl) != 0);
				CachedDirectWebResponse response = this.chainedWebRequestHandler.GetResponse(request, options & ~DirectWebRequestOptions.RequireSsl).GetSnapshot(this.MaximumBytesToRead);
				if (response.Status == HttpStatusCode.MovedPermanently ||
					response.Status == HttpStatusCode.Redirect ||
					response.Status == HttpStatusCode.RedirectMethod ||
					response.Status == HttpStatusCode.RedirectKeepVerb) {
					// We have no copy of the post entity stream to repeat on our manually
					// cloned HttpWebRequest, so we have to bail.
					ErrorUtilities.VerifyProtocol(request.Method != "POST", MessagingStrings.UntrustedRedirectsOnPOSTNotSupported);
					Uri redirectUri = new Uri(response.FinalUri, response.Headers[HttpResponseHeader.Location]);
					request = request.Clone(redirectUri);
				} else {
					if (response.FinalUri != request.RequestUri) {
						// Since we don't automatically follow redirects, there's only one scenario where this
						// can happen: when the server sends a (non-redirecting) Content-Location header in the response.
						// It's imperative that we do not trust that header though, so coerce the FinalUri to be
						// what we just requested.
						Logger.Http.WarnFormat("The response from {0} included an HTTP header indicating it's the same as {1}, but it's not a redirect so we won't trust that.", request.RequestUri, response.FinalUri);
						response.FinalUri = request.RequestUri;
					}

					return response;
				}
			}

			throw ErrorUtilities.ThrowProtocol(MessagingStrings.TooManyRedirects, originalRequestUri);
		}

		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <returns>
		/// The writer the caller should write out the entity data to.
		/// </returns>
		Stream IDirectWebRequestHandler.GetRequestStream(HttpWebRequest request) {
			return this.GetRequestStream(request, DirectWebRequestOptions.None);
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the
		/// <see cref="HttpWebResponse"/> to a <see cref="IncomingWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>
		/// An instance of <see cref="IncomingWebResponse"/> describing the response.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.  The <see cref="WebException.Response"/>
		/// value, if set, should be Closed before throwing.</para>
		/// </remarks>
		IncomingWebResponse IDirectWebRequestHandler.GetResponse(HttpWebRequest request) {
			return this.GetResponse(request, DirectWebRequestOptions.None);
		}

		#endregion

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
		/// <param name="requireSsl">If set to <c>true</c>, only web requests that can be made entirely over SSL will succeed.</param>
		/// <exception cref="ProtocolException">Thrown when the URI is disallowed for security reasons.</exception>
		private void EnsureAllowableRequestUri(Uri requestUri, bool requireSsl) {
			ErrorUtilities.VerifyProtocol(this.IsUriAllowable(requestUri), MessagingStrings.UnsafeWebRequestDetected, requestUri);
			ErrorUtilities.VerifyProtocol(!requireSsl || string.Equals(requestUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase), MessagingStrings.InsecureWebRequestWithSslRequired, requestUri);
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

		/// <summary>
		/// Prepares the request by setting timeout and redirect policies.
		/// </summary>
		/// <param name="request">The request to prepare.</param>
		/// <param name="preparingPost"><c>true</c> if this is a POST request whose headers have not yet been sent out; <c>false</c> otherwise.</param>
		private void PrepareRequest(HttpWebRequest request, bool preparingPost) {
			Requires.NotNull(request, "request");

			// Be careful to not try to change the HTTP headers that have already gone out.
			if (preparingPost || request.Method == "GET") {
				// Set/override a few properties of the request to apply our policies for untrusted requests.
				request.ReadWriteTimeout = (int)this.ReadWriteTimeout.TotalMilliseconds;
				request.Timeout = (int)this.Timeout.TotalMilliseconds;
				request.KeepAlive = false;
			}

			// If SSL is required throughout, we cannot allow auto redirects because
			// it may include a pass through an unprotected HTTP request.
			// We have to follow redirects manually.
			// It also allows us to ignore HttpWebResponse.FinalUri since that can be affected by
			// the Content-Location header and open security holes.
			request.AllowAutoRedirect = false;
		}
	}
}
