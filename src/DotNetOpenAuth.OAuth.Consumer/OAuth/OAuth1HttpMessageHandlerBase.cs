//-----------------------------------------------------------------------
// <copyright file="OAuth1HttpMessageHandlerBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Globalization;
	using System.Linq;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A base class for delegating <see cref="HttpMessageHandler" />s that sign
	/// outgoing HTTP requests per the OAuth 1.0 "3.4 Signature" in RFC 5849.
	/// </summary>
	/// <remarks>
	/// An implementation of http://tools.ietf.org/html/rfc5849#section-3.4
	/// </remarks>
	public abstract class OAuth1HttpMessageHandlerBase : DelegatingHandler {
		/// <summary>
		/// These are the characters that may be chosen from when forming a random nonce.
		/// </summary>
		private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

		/// <summary>
		/// The default nonce length.
		/// </summary>
		private const int DefaultNonceLength = 8;

		/// <summary>
		/// The default parameters location.
		/// </summary>
		private const OAuthParametersLocation DefaultParametersLocation = OAuthParametersLocation.AuthorizationHttpHeader;

		/// <summary>
		/// The reference date and time for calculating time stamps.
		/// </summary>
		private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// An array containing simply the amperstand character.
		/// </summary>
		private static readonly char[] ParameterSeparatorAsArray = new char[] { '&' };

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth1HttpMessageHandlerBase"/> class.
		/// </summary>
		protected OAuth1HttpMessageHandlerBase() {
			this.NonceLength = DefaultNonceLength;
			this.Location = DefaultParametersLocation;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth1HttpMessageHandlerBase"/> class.
		/// </summary>
		/// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
		protected OAuth1HttpMessageHandlerBase(HttpMessageHandler innerHandler)
			: base(innerHandler) {
			this.NonceLength = DefaultNonceLength;
			this.Location = DefaultParametersLocation;
		}

		/// <summary>
		/// The locations that oauth parameters may be added to HTTP requests.
		/// </summary>
		public enum OAuthParametersLocation {
			/// <summary>
			/// The oauth parameters are added to the query string in the URL.
			/// </summary>
			QueryString,

			/// <summary>
			/// An HTTP Authorization header is added with the OAuth scheme.
			/// </summary>
			AuthorizationHttpHeader,
		}

		/// <summary>
		/// Gets or sets the location to add OAuth parameters to outbound HTTP requests.
		/// </summary>
		public OAuthParametersLocation Location { get; set; }

		/// <summary>
		/// Gets or sets the consumer key.
		/// </summary>
		/// <value>
		/// The consumer key.
		/// </value>
		public string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the consumer secret.
		/// </summary>
		/// <value>
		/// The consumer secret.
		/// </value>
		public string ConsumerSecret { get; set; }

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>
		/// The access token.
		/// </value>
		public string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the access token secret.
		/// </summary>
		/// <value>
		/// The access token secret.
		/// </value>
		public string AccessTokenSecret { get; set; }

		/// <summary>
		/// Gets or sets the length of the nonce.
		/// </summary>
		/// <value>
		/// The length of the nonce.
		/// </value>
		public int NonceLength { get; set; }

		/// <summary>
		/// Gets the signature method to include in the oauth_signature_method parameter.
		/// </summary>
		/// <value>
		/// The signature method.
		/// </value>
		protected abstract string SignatureMethod { get; }

		/// <summary>
		/// Applies OAuth authorization to the specified request.
		/// This method is applied automatically to outbound requests that use this message handler instance.
		/// However this method may be useful for obtaining the OAuth 1.0 signature without actually sending the request.
		/// </summary>
		/// <param name="request">The request.</param>
		public void ApplyAuthorization(HttpRequestMessage request) {
			Requires.NotNull(request, "request");

			var oauthParameters = this.GetOAuthParameters();
			string signature = this.GetSignature(request, oauthParameters);
			oauthParameters.Add("oauth_signature", signature);

			// Add parameters and signature to request.
			switch (this.Location) {
				case OAuthParametersLocation.AuthorizationHttpHeader:
					// Some oauth parameters may have been put in the query string of the original message.
					// We want to move any that we find into the authorization header.
					oauthParameters.Add(ExtractOAuthParametersFromQueryString(request));

					request.Headers.Authorization = new AuthenticationHeaderValue(Protocol.AuthorizationHeaderScheme, MessagingUtilities.AssembleAuthorizationHeader(oauthParameters.AsKeyValuePairs()));
					break;
				case OAuthParametersLocation.QueryString:
					var uriBuilder = new UriBuilder(request.RequestUri);
					uriBuilder.AppendQueryArgs(oauthParameters.AsKeyValuePairs());
					request.RequestUri = uriBuilder.Uri;
					break;
			}
		}

		/// <summary>
		/// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
		/// </summary>
		/// <param name="request">The HTTP request message to send to the server.</param>
		/// <param name="cancellationToken">A cancellation token to cancel operation.</param>
		/// <returns>
		/// Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.
		/// </returns>
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");
			cancellationToken.ThrowIfCancellationRequested();
			this.ApplyAuthorization(request);
			return base.SendAsync(request, cancellationToken);
		}

		/// <summary>
		/// Calculates the signature for the specified buffer.
		/// </summary>
		/// <param name="signedPayload">The payload to calculate the signature for.</param>
		/// <returns>The signature.</returns>
		protected abstract byte[] Sign(byte[] signedPayload);

		/// <summary>
		/// Gets the OAuth 1.0 signature to apply to the specified request.
		/// </summary>
		/// <param name="request">The outbound HTTP request.</param>
		/// <param name="oauthParameters">The oauth parameters.</param>
		/// <returns>
		/// The value for the "oauth_signature" parameter.
		/// </returns>
		protected virtual string GetSignature(HttpRequestMessage request, NameValueCollection oauthParameters) {
			Requires.NotNull(request, "request");
			Requires.NotNull(oauthParameters, "oauthParameters");

			string signatureBaseString = this.ConstructSignatureBaseString(request, oauthParameters);
			byte[] signatureBaseStringBytes = Encoding.ASCII.GetBytes(signatureBaseString);
			byte[] signatureBytes = this.Sign(signatureBaseStringBytes);
			string signatureString = Convert.ToBase64String(signatureBytes);
			return signatureString;
		}

		/// <summary>
		/// Gets the "ConsumerSecret&amp;AccessTokenSecret" string, allowing either property to be empty or null.
		/// </summary>
		/// <returns>The concatenated string.</returns>
		/// <remarks>
		/// This is useful in the PLAINTEXT and HMAC-SHA1 signature algorithms.
		/// </remarks>
		protected string GetConsumerAndTokenSecretString() {
			var builder = new StringBuilder();
			builder.Append(UrlEscape(this.ConsumerSecret ?? string.Empty));
			builder.Append("&");
			builder.Append(UrlEscape(this.AccessTokenSecret ?? string.Empty));
			return builder.ToString();
		}

		/// <summary>
		/// Escapes a value for transport in a URI, per RFC 3986.
		/// </summary>
		/// <param name="value">The value to escape. Null and empty strings are OK.</param>
		/// <returns>The escaped value. Never null.</returns>
		private static string UrlEscape(string value) {
			return MessagingUtilities.EscapeUriDataStringRfc3986(value ?? string.Empty);
		}

		/// <summary>
		/// Returns the OAuth 1.0 timestamp for the current time.
		/// </summary>
		/// <param name="dateTime">The date time.</param>
		/// <returns>A string representation of the number of seconds since "the epoch".</returns>
		private static string ToTimeStamp(DateTime dateTime) {
			Requires.Argument(dateTime.Kind == DateTimeKind.Utc, "dateTime", "UTC time required");
			TimeSpan ts = dateTime - epoch;
			long secondsSinceEpoch = (long)ts.TotalSeconds;
			return secondsSinceEpoch.ToString(CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Constructs the "Base String URI" as described in http://tools.ietf.org/html/rfc5849#section-3.4.1.2
		/// </summary>
		/// <param name="requestUri">The request URI.</param>
		/// <returns>
		/// The string to include in the signature base string.
		/// </returns>
		private static string GetBaseStringUri(Uri requestUri) {
			Requires.NotNull(requestUri, "requestUri");

			var endpoint = new UriBuilder(requestUri);
			endpoint.Query = null;
			endpoint.Fragment = null;
			return endpoint.Uri.AbsoluteUri;
		}

		/// <summary>
		/// Collects and removes all query string parameters beginning with "oauth_" from the specified request,
		/// and returns them as a collection.
		/// </summary>
		/// <param name="request">The request whose query string should be searched for "oauth_" parameters.</param>
		/// <returns>The collection of parameters that were removed from the query string.</returns>
		private static NameValueCollection ExtractOAuthParametersFromQueryString(HttpRequestMessage request) {
			Requires.NotNull(request, "request");

			var extracted = new NameValueCollection();
			if (!string.IsNullOrEmpty(request.RequestUri.Query)) {
				var queryString = HttpUtility.ParseQueryString(request.RequestUri.Query);
				foreach (var pair in queryString.AsKeyValuePairs()) {
					if (pair.Key.StartsWith(Protocol.ParameterPrefix, StringComparison.Ordinal)) {
						extracted.Add(pair.Key, pair.Value);
					}
				}

				if (extracted.Count > 0) {
					foreach (string key in extracted) {
						queryString.Remove(key);
					}

					var modifiedRequestUri = new UriBuilder(request.RequestUri);
					modifiedRequestUri.Query = MessagingUtilities.CreateQueryString(queryString.AsKeyValuePairs());
					request.RequestUri = modifiedRequestUri.Uri;
				}
			}

			return extracted;
		}

		/// <summary>
		/// Constructs the "Signature Base String" as described in http://tools.ietf.org/html/rfc5849#section-3.4.1
		/// </summary>
		/// <param name="request">The HTTP request message.</param>
		/// <param name="oauthParameters">The oauth parameters.</param>
		/// <returns>
		/// The signature base string.
		/// </returns>
		private string ConstructSignatureBaseString(HttpRequestMessage request, NameValueCollection oauthParameters) {
			Requires.NotNull(request, "request");
			Requires.NotNull(oauthParameters, "oauthParameters");

			var builder = new StringBuilder();
			builder.Append(UrlEscape(request.Method.ToString().ToUpperInvariant()));
			builder.Append("&");
			builder.Append(UrlEscape(GetBaseStringUri(request.RequestUri)));
			builder.Append("&");
			builder.Append(UrlEscape(this.GetNormalizedParameters(request, oauthParameters)));

			return builder.ToString();
		}

		/// <summary>
		/// Generates a string of random characters for use as a nonce.
		/// </summary>
		/// <returns>The nonce string.</returns>
		private string GenerateUniqueFragment() {
			return MessagingUtilities.GetRandomString(this.NonceLength, AllowedCharacters);
		}

		/// <summary>
		/// Gets the "oauth_" prefixed parameters that should be added to an outbound request.
		/// </summary>
		/// <returns>A collection of name=value pairs.</returns>
		private NameValueCollection GetOAuthParameters() {
			var nvc = new NameValueCollection(8);
			nvc.Add("oauth_version", "1.0");
			nvc.Add("oauth_nonce", this.GenerateUniqueFragment());
			nvc.Add("oauth_timestamp", ToTimeStamp(DateTime.UtcNow));
			nvc.Add("oauth_signature_method", this.SignatureMethod);
			nvc.Add("oauth_consumer_key", this.ConsumerKey);
			if (!string.IsNullOrEmpty(this.AccessToken)) {
				nvc.Add("oauth_token", this.AccessToken);
			}

			return nvc;
		}

		/// <summary>
		/// Gets a normalized string of the query string parameters included in the request and the additional OAuth parameters.
		/// </summary>
		/// <param name="request">The HTTP request.</param>
		/// <param name="oauthParameters">The oauth parameters that will be added to the request.</param>
		/// <returns>The normalized string of parameters to included in the signature base string.</returns>
		private string GetNormalizedParameters(HttpRequestMessage request, NameValueCollection oauthParameters) {
			Requires.NotNull(request, "request");
			Requires.NotNull(oauthParameters, "oauthParameters");

			NameValueCollection nvc;
			if (request.RequestUri.Query != null) {
				// NameValueCollection does support non-unique keys, as long as you use it carefully.
				nvc = HttpUtility.ParseQueryString(request.RequestUri.Query);
			} else {
				nvc = new NameValueCollection(8);
			}

			// Add OAuth parameters.
			nvc.Add(oauthParameters);

			// Now convert the NameValueCollection into an ordered list, and properly escape all keys and value while we're at it.
			var list = new List<KeyValuePair<string, string>>(nvc.Count);
			foreach (var pair in nvc.AsKeyValuePairs()) {
				string escapedKey = UrlEscape(pair.Key);
				string escapedValue = UrlEscape(pair.Value ?? string.Empty); // value can be null if no "=" appears in the query string for this key.
				list.Add(new KeyValuePair<string, string>(escapedKey, escapedValue));
			}

			// Sort the parameters
			list.Sort((kv1, kv2) => {
				int compare = string.Compare(kv1.Key, kv2.Key, StringComparison.Ordinal);
				if (compare != 0) {
					return compare;
				}

				return string.Compare(kv1.Value, kv2.Value, StringComparison.Ordinal);
			});

			// Convert this sorted list into a single concatenated string.
			var normalizedParameterString = new StringBuilder();
			foreach (var pair in list) {
				if (normalizedParameterString.Length > 0) {
					normalizedParameterString.Append("&");
				}

				normalizedParameterString.Append(pair.Key);
				normalizedParameterString.Append("=");
				normalizedParameterString.Append(pair.Value);
			}

			return normalizedParameterString.ToString();
		}
	}
}
