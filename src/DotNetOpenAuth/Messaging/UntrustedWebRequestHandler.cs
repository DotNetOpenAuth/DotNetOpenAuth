//-----------------------------------------------------------------------
// <copyright file="UntrustedWebRequestHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if DEBUG
#define LONGTIMEOUT
#endif
namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
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
	public class UntrustedWebRequestHandler : IDirectSslWebRequestHandler {
		/// <summary>
		/// Gets or sets the default cache policy to use for HTTP requests.
		/// </summary>
		internal readonly static RequestCachePolicy DefaultCachePolicy = HttpWebRequest.DefaultCachePolicy;

		private static DotNetOpenAuth.Configuration.UntrustedWebRequestSection Configuration {
			get { return UntrustedWebRequestSection.Configuration; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int maximumBytesToRead = Configuration.MaximumBytesToRead;

		/// <summary>
		/// The default maximum bytes to read in any given HTTP request.
		/// Default is 1MB.  Cannot be less than 2KB.
		/// </summary>
		public int MaximumBytesToRead {
			get { return maximumBytesToRead; }
			set {
				if (value < 2048) throw new ArgumentOutOfRangeException("value");
				maximumBytesToRead = value;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int maximumRedirections = Configuration.MaximumRedirections;

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
			ErrorUtilities.VerifyArgumentNotNull(chainedWebRequestHandler, "chainedWebRequestHandler");

			this.chainedWebRequestHandler = chainedWebRequestHandler;
			this.ReadWriteTimeout = Configuration.ReadWriteTimeout;
			this.Timeout = Configuration.Timeout;
#if LONGTIMEOUT
			this.ReadWriteTimeout = TimeSpan.FromHours(1);
			this.Timeout = TimeSpan.FromHours(1);
#endif
		}

		/// <summary>
		/// Gets or sets the total number of redirections to allow on any one request.
		/// Default is 10.
		/// </summary>
		public int MaximumRedirections {
			get { return maximumRedirections; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("value");
				maximumRedirections = value;
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

		private ICollection<string> allowableSchemes = new List<string> { "http", "https" };
		private ICollection<string> whitelistHosts = new List<string>(Configuration.WhitelistHosts.KeysAsStrings);
		/// <summary>
		/// A collection of host name literals that should be allowed even if they don't
		/// pass standard security checks.
		/// </summary>
		public ICollection<string> WhitelistHosts { get { return whitelistHosts; } }
		private ICollection<Regex> whitelistHostsRegex = new List<Regex>(Configuration.WhitelistHostsRegex.KeysAsRegexs);
		/// <summary>
		/// A collection of host name regular expressions that indicate hosts that should
		/// be allowed even though they don't pass standard security checks.
		/// </summary>
		public ICollection<Regex> WhitelistHostsRegex { get { return whitelistHostsRegex; } }
		private ICollection<string> blacklistHosts = new List<string>(Configuration.BlacklistHosts.KeysAsStrings);
		/// <summary>
		/// A collection of host name literals that should be rejected even if they 
		/// pass standard security checks.
		/// </summary>
		public ICollection<string> BlacklistHosts { get { return blacklistHosts; } }
		private ICollection<Regex> blacklistHostsRegex = new List<Regex>(Configuration.BlacklistHostsRegex.KeysAsRegexs);
		/// <summary>
		/// A collection of host name regular expressions that indicate hosts that should
		/// be rjected even if they pass standard security checks.
		/// </summary>
		public ICollection<Regex> BlacklistHostsRegex { get { return blacklistHostsRegex; } }

		#region IDirectSslWebRequestHandler Members

		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <param name="requireSsl">if set to <c>true</c> all requests made with this instance must be completed using SSL.</param>
		/// <returns>
		/// The writer the caller should write out the entity data to.
		/// </returns>
		public TextWriter GetRequestStream(HttpWebRequest request, bool requireSsl) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			this.EnsureAllowableRequestUri(request.RequestUri, requireSsl);

			this.PrepareRequest(request);

			// We don't currently support redirects at URLs where we're POSTing data.
			// When we want to add this support, we need to be careful to not allow
			// redirects to non-HTTPS schemes if RequireSsl is true.
			request.AllowAutoRedirect = false;

			// Submit the request and get the request stream back.
			return this.chainedWebRequestHandler.GetRequestStream(request);
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the
		/// <see cref="HttpWebResponse"/> to a <see cref="DirectWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <param name="requireSsl">if set to <c>true</c> all requests made with this instance must be completed using SSL.</param>
		/// <returns>
		/// An instance of <see cref="DirectWebResponse"/> describing the response.
		/// </returns>
		public DirectWebResponse GetResponse(HttpWebRequest request, bool requireSsl) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			this.EnsureAllowableRequestUri(request.RequestUri, requireSsl);

			// This request MAY have already been prepared by GetRequestStream, but
			// we have no guarantee, so do it just to be safe.
			this.PrepareRequest(request);

			return this.RequestWithManagedRedirects(request, requireSsl);
		}

		#endregion

		#region IDirectWebRequestHandler Members

		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <returns>
		/// The writer the caller should write out the entity data to.
		/// </returns>
		TextWriter IDirectWebRequestHandler.GetRequestStream(HttpWebRequest request) {
			return this.GetRequestStream(request, false);
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the 
		/// <see cref="HttpWebResponse"/> to a <see cref="DirectWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>An instance of <see cref="DirectWebResponse"/> describing the response.</returns>
		DirectWebResponse IDirectWebRequestHandler.GetResponse(HttpWebRequest request) {
			return this.GetResponse(request, false);
		}

		#endregion

		internal DirectWebResponse RequestWithManagedRedirects(HttpWebRequest request, bool requireSsl) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			// Since we may require SSL for every redirect, we handle each redirect manually
			// in order to detect and fail if any redirect sends us to an HTTP url.
			// We COULD allow automatic redirect in the cases where HTTPS is not required,
			// but our mock request infrastructure can't do redirects on its own either.
			Uri originalRequestUri = request.RequestUri;
			int i;
			for (i = 0; i < MaximumRedirections; i++) {
				DirectWebResponse response = this.RequestCore(request, null, originalRequestUri, requireSsl);
				if (response.Status == HttpStatusCode.MovedPermanently ||
					response.Status == HttpStatusCode.Redirect ||
					response.Status == HttpStatusCode.RedirectMethod ||
					response.Status == HttpStatusCode.RedirectKeepVerb) {
					Uri redirectUri = new Uri(response.FinalUri, response.Headers[HttpResponseHeader.Location]);
					request = CloneRequestWithNewUrl(request, redirectUri);
				} else {
					return response;
				}
			}
			throw new WebException(string.Format(CultureInfo.CurrentCulture, MessagingStrings.TooManyRedirects, originalRequestUri));
		}

		private static HttpWebRequest CloneRequestWithNewUrl(HttpWebRequest request, Uri newRequestUri) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			ErrorUtilities.VerifyArgumentNotNull(newRequestUri, "newRequestUri");

			var newRequest = (HttpWebRequest)WebRequest.Create(newRequestUri);
			newRequest.Accept = request.Accept;
			newRequest.AllowAutoRedirect = request.AllowAutoRedirect;
			newRequest.AllowWriteStreamBuffering = request.AllowWriteStreamBuffering;
			newRequest.AuthenticationLevel = request.AuthenticationLevel;
			newRequest.AutomaticDecompression = request.AutomaticDecompression;
			newRequest.CachePolicy = request.CachePolicy;
			newRequest.ClientCertificates = request.ClientCertificates;
			newRequest.ConnectionGroupName = request.ConnectionGroupName;
			if (request.ContentLength >= 0) {
				newRequest.ContentLength = request.ContentLength;
			}
			newRequest.ContentType = request.ContentType;
			newRequest.ContinueDelegate = request.ContinueDelegate;
			newRequest.CookieContainer = request.CookieContainer;
			newRequest.Credentials = request.Credentials;
			newRequest.Expect = request.Expect;
			newRequest.IfModifiedSince = request.IfModifiedSince;
			newRequest.ImpersonationLevel = request.ImpersonationLevel;
			newRequest.KeepAlive = request.KeepAlive;
			newRequest.MaximumAutomaticRedirections = request.MaximumAutomaticRedirections;
			newRequest.MaximumResponseHeadersLength = request.MaximumResponseHeadersLength;
			newRequest.MediaType = request.MediaType;
			newRequest.Method = request.Method;
			newRequest.Pipelined = request.Pipelined;
			newRequest.PreAuthenticate = request.PreAuthenticate;
			newRequest.ProtocolVersion = request.ProtocolVersion;
			newRequest.Proxy = request.Proxy;
			newRequest.ReadWriteTimeout = request.ReadWriteTimeout;
			newRequest.Referer = request.Referer;
			newRequest.SendChunked = request.SendChunked;
			newRequest.Timeout = request.Timeout;
			newRequest.TransferEncoding = request.TransferEncoding;
			newRequest.UnsafeAuthenticatedConnectionSharing = request.UnsafeAuthenticatedConnectionSharing;
			newRequest.UseDefaultCredentials = request.UseDefaultCredentials;
			newRequest.UserAgent = request.UserAgent;

			// We copy headers last, and only those that do not yet exist as a result
			// of setting these properties, so as to avoid exceptions thrown because 
			// there are properties .NET wants us to use rather than direct headers.
			foreach (string header in request.Headers) {
				if (string.IsNullOrEmpty(newRequest.Headers[header])) {
					newRequest.Headers.Add(header, request.Headers[header]);
				}
			}

			return newRequest;
		}

		private bool isHostWhitelisted(string host) {
			return isHostInList(host, WhitelistHosts, WhitelistHostsRegex);
		}

		private bool isHostBlacklisted(string host) {
			return isHostInList(host, BlacklistHosts, BlacklistHostsRegex);
		}

		private bool isHostInList(string host, ICollection<string> stringList, ICollection<Regex> regexList) {
			Debug.Assert(!string.IsNullOrEmpty(host));
			Debug.Assert(stringList != null);
			Debug.Assert(regexList != null);
			foreach (string testHost in stringList) {
				if (string.Equals(host, testHost, StringComparison.OrdinalIgnoreCase))
					return true;
			}
			foreach (Regex regex in regexList) {
				if (regex.IsMatch(host))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Verify that the request qualifies under our security policies
		/// </summary>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="requireSsl">If set to <c>true</c>, only web requests that can be made entirely over SSL will succeed.</param>
		private void EnsureAllowableRequestUri(Uri requestUri, bool requireSsl) {
			ErrorUtilities.VerifyArgument(this.isUriAllowable(requestUri), MessagingStrings.UnsafeWebRequestDetected, requestUri);

			ErrorUtilities.VerifyProtocol(!requireSsl || String.Equals(requestUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase), MessagingStrings.InsecureWebRequestWithSslRequired, requestUri);
		}

		private bool isUriAllowable(Uri uri) {
			Debug.Assert(uri != null);
			if (!allowableSchemes.Contains(uri.Scheme)) {
				Logger.WarnFormat("Rejecting URL {0} because it uses a disallowed scheme.", uri);
				return false;
			}

			// Allow for whitelist or blacklist to override our detection.
			Func<string, bool> failsUnlessWhitelisted = (string reason) => {
				if (isHostWhitelisted(uri.DnsSafeHost)) return true;
				Logger.WarnFormat("Rejecting URL {0} because {1}.", uri, reason);
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
						if (addressBytes[0] == 127 || addressBytes[0] == 10)
							return failsUnlessWhitelisted("it is a loopback address.");
						break;
					case System.Net.Sockets.AddressFamily.InterNetworkV6:
						if (isIPv6Loopback(hostIPAddress))
							return failsUnlessWhitelisted("it is a loopback address.");
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
			if (isHostBlacklisted(uri.DnsSafeHost)) {
				Logger.WarnFormat("Rejected URL {0} because it is blacklisted.", uri);
				return false;
			}
			return true;
		}

		private bool isIPv6Loopback(IPAddress ip) {
			Debug.Assert(ip != null);
			byte[] addressBytes = ip.GetAddressBytes();
			for (int i = 0; i < addressBytes.Length - 1; i++)
				if (addressBytes[i] != 0) return false;
			if (addressBytes[addressBytes.Length - 1] != 1) return false;
			return true;
		}

		private HttpWebRequest PrepareRequest(HttpWebRequest request) {
			// Set/override a few properties of the request to apply our policies for untrusted requests.
			request.ReadWriteTimeout = (int)ReadWriteTimeout.TotalMilliseconds;
			request.Timeout = (int)Timeout.TotalMilliseconds;
			request.KeepAlive = false;

			// If SSL is required throughout, we cannot allow auto redirects because
			// it may include a pass through an unprotected HTTP request.
			// We have to follow redirects manually.
			request.AllowAutoRedirect = false;

			return request;
		}

		private DirectWebResponse RequestCore(HttpWebRequest request, Stream postEntity, Uri originalRequestUri, bool requireSsl) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			ErrorUtilities.VerifyArgumentNotNull(originalRequestUri, "originalRequestUri");
			EnsureAllowableRequestUri(request.RequestUri, requireSsl);

			int postEntityLength = 0;
			try {
				if (postEntity != null) {
					using (Stream outStream = request.GetRequestStream()) {
						postEntityLength = postEntity.CopyTo(outStream);
					}
				}

				DirectWebResponse response = this.chainedWebRequestHandler.GetResponse(request);
				response.CacheNetworkStreamAndClose(MaximumBytesToRead);
				return response;
			} catch (WebException e) {
				using (HttpWebResponse response = (HttpWebResponse)e.Response) {
					if (response != null) {
						if (response.StatusCode == HttpStatusCode.ExpectationFailed) {
							if (request.ServicePoint.Expect100Continue) { // must only try this once more
								// Some OpenID servers doesn't understand the Expect header and send 417 error back.
								// If this server just failed from that, we're trying again without sending the
								// "Expect: 100-Continue" HTTP header. (see Google Code Issue 72)
								// We don't just set Expect100Continue = !avoidSendingExpect100Continue
								// so that future requests don't reset this and have to try twice as well.
								// We don't want to blindly set all ServicePoints to not use the Expect header
								// as that would be a security hole allowing any visitor to a web site change
								// the web site's global behavior when calling that host.
								request.ServicePoint.Expect100Continue = false; // TODO: investigate that CAS may throw here, and we can use request.Expect instead.
								postEntity.Seek(-postEntityLength, SeekOrigin.Current);
								request = CloneRequestWithNewUrl(request, request.RequestUri);
								return RequestCore(request, postEntity, originalRequestUri, requireSsl);
							}
						}
						var directResponse = new DirectWebResponse(originalRequestUri, response);
						directResponse.CacheNetworkStreamAndClose(MaximumBytesToRead);
						return directResponse;
					} else {
						throw ErrorUtilities.Wrap(e, MessagingStrings.WebRequestFailed, originalRequestUri);
					}
				}
			}
		}
	}
}
