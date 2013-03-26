//-----------------------------------------------------------------------
// <copyright file="HttpRequestInfo.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Net.Mime;
	using System.ServiceModel.Channels;
	using System.Web;
	using Validation;

	/// <summary>
	/// A property store of details of an incoming HTTP request.
	/// </summary>
	/// <remarks>
	/// This serves a very similar purpose to <see cref="HttpRequest"/>, except that
	/// ASP.NET does not let us fully initialize that class, so we have to write one
	/// of our one.
	/// </remarks>
	public class HttpRequestInfo : HttpRequestBase {
		/// <summary>
		/// The HTTP verb in the request.
		/// </summary>
		private readonly string httpMethod;

		/// <summary>
		/// The full request URL.
		/// </summary>
		private readonly Uri requestUri;

		/// <summary>
		/// The HTTP headers.
		/// </summary>
		private readonly NameValueCollection headers;

		/// <summary>
		/// The variables defined in the query part of the URL.
		/// </summary>
		private readonly NameValueCollection queryString;

		/// <summary>
		/// The POSTed form variables.
		/// </summary>
		private readonly NameValueCollection form;

		/// <summary>
		/// The server variables collection.
		/// </summary>
		private readonly NameValueCollection serverVariables;

		/// <summary>
		/// The backing field for the <see cref="Cookies"/> property.
		/// </summary>
		private readonly HttpCookieCollection cookies;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="requestUri">The request URI.</param>
		internal HttpRequestInfo(HttpRequestMessageProperty request, Uri requestUri) {
			Requires.NotNull(request, "request");
			Requires.NotNull(requestUri, "requestUri");

			this.httpMethod = request.Method;
			this.headers = request.Headers;
			this.requestUri = requestUri;
			this.form = new NameValueCollection();
			this.queryString = HttpUtility.ParseQueryString(requestUri.Query);
			this.serverVariables = new NameValueCollection();
			this.cookies = new HttpCookieCollection();

			Reporting.RecordRequestStatistics(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo"/> class.
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="form">The form variables.</param>
		/// <param name="headers">The HTTP headers.</param>
		/// <param name="cookies">The cookies in the request.</param>
		internal HttpRequestInfo(string httpMethod, Uri requestUri, NameValueCollection form = null, NameValueCollection headers = null, HttpCookieCollection cookies = null) {
			Requires.NotNullOrEmpty(httpMethod, "httpMethod");
			Requires.NotNull(requestUri, "requestUri");

			this.httpMethod = httpMethod;
			this.requestUri = requestUri;
			this.form = form ?? new NameValueCollection();
			this.queryString = HttpUtility.ParseQueryString(requestUri.Query);
			this.headers = headers ?? new WebHeaderCollection();
			this.serverVariables = new NameValueCollection();
			this.cookies = cookies ?? new HttpCookieCollection();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo"/> class.
		/// </summary>
		/// <param name="listenerRequest">Details on the incoming HTTP request.</param>
		internal HttpRequestInfo(HttpListenerRequest listenerRequest) {
			Requires.NotNull(listenerRequest, "listenerRequest");

			this.httpMethod = listenerRequest.HttpMethod;
			this.requestUri = listenerRequest.Url;
			this.queryString = listenerRequest.QueryString;
			this.headers = listenerRequest.Headers;
			this.form = ParseFormData(listenerRequest.HttpMethod, listenerRequest.Headers, () => listenerRequest.InputStream);
			this.serverVariables = new NameValueCollection();
			this.cookies = new HttpCookieCollection();

			Reporting.RecordRequestStatistics(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo" /> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal HttpRequestInfo(HttpRequestMessage request) {
			Requires.NotNull(request, "request");

			this.httpMethod = request.Method.ToString();
			this.requestUri = request.RequestUri;
			this.queryString = HttpUtility.ParseQueryString(request.RequestUri.Query);
			this.headers = new NameValueCollection();
			AddHeaders(this.headers, request.Headers);
			AddHeaders(this.headers, request.Content.Headers);
			this.form = ParseFormData(this.httpMethod, this.headers, () => request.Content.ReadAsStreamAsync().Result);
			this.serverVariables = new NameValueCollection();
			this.cookies = new HttpCookieCollection();

			Reporting.RecordRequestStatistics(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo"/> class.
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="headers">The headers.</param>
		/// <param name="inputStream">The input stream.</param>
		internal HttpRequestInfo(string httpMethod, Uri requestUri, NameValueCollection headers, Stream inputStream) {
			Requires.NotNullOrEmpty(httpMethod, "httpMethod");
			Requires.NotNull(requestUri, "requestUri");

			this.httpMethod = httpMethod;
			this.requestUri = requestUri;
			this.headers = headers;
			this.queryString = HttpUtility.ParseQueryString(requestUri.Query);
			this.form = ParseFormData(httpMethod, headers, () => inputStream);
			this.serverVariables = new NameValueCollection();
			this.cookies = new HttpCookieCollection();

			Reporting.RecordRequestStatistics(this);
		}

		/// <summary>
		/// Gets the HTTP method.
		/// </summary>
		public override string HttpMethod {
			get { return this.httpMethod; }
		}

		/// <summary>
		/// Gets the headers.
		/// </summary>
		public override NameValueCollection Headers {
			get { return this.headers; }
		}

		/// <summary>
		/// Gets the URL.
		/// </summary>
		public override Uri Url {
			get { return this.requestUri; }
		}

		/// <summary>
		/// Gets the raw URL.
		/// </summary>
		public override string RawUrl {
			get { return this.requestUri.AbsolutePath + this.requestUri.Query; }
		}

		/// <summary>
		/// Gets the form.
		/// </summary>
		public override NameValueCollection Form {
			get { return this.form; }
		}

		/// <summary>
		/// Gets the query string.
		/// </summary>
		public override NameValueCollection QueryString {
			get { return this.queryString; }
		}

		/// <summary>
		/// Gets the server variables.
		/// </summary>
		public override NameValueCollection ServerVariables {
			get { return this.serverVariables; }
		}

		/// <summary>
		/// Gets the collection of cookies that were sent by the client.
		/// </summary>
		/// <returns>The client's cookies.</returns>
		public override HttpCookieCollection Cookies {
			get { return this.cookies; }
		}

		/// <summary>
		/// When overridden in a derived class, gets an array of client-supported MIME accept types.
		/// </summary>
		/// <returns>An array of client-supported MIME accept types.</returns>
		public override string[] AcceptTypes {
			get {
				if (this.Headers["Accept"] != null) {
					return this.headers["Accept"].Split(',');
				}

				return new string[0];
			}
		}

		/// <summary>
		/// When overridden in a derived class, gets information about the URL of the client request that linked to the current URL.
		/// </summary>
		/// <returns>The URL of the page that linked to the current request.</returns>
		public override Uri UrlReferrer {
			get {
				if (this.Headers["Referer"] != null) { // misspelled word intentional, per RFC
					return new Uri(this.Headers["Referer"]);
				}

				return null;
			}
		}

		/// <summary>
		/// When overridden in a derived class, gets the length, in bytes, of content that was sent by the client.
		/// </summary>
		/// <returns>The length, in bytes, of content that was sent by the client.</returns>
		public override int ContentLength {
			get {
				if (this.Headers["Content-Length"] != null) {
					return int.Parse(this.headers["Content-Length"]);
				}

				return 0;
			}
		}

		/// <summary>
		/// When overridden in a derived class, gets or sets the MIME content type of the request.
		/// </summary>
		/// <returns>The MIME content type of the request, such as "text/html".</returns>
		/// <exception cref="System.NotImplementedException">Setter always throws</exception>
		public override string ContentType {
			get { return this.Headers["Content-Type"]; }
			set { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Creates an <see cref="HttpRequestBase"/> instance that describes the specified HTTP request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="requestUri">The request URI.</param>
		/// <returns>An instance of <see cref="HttpRequestBase"/>.</returns>
		public static HttpRequestBase Create(HttpRequestMessageProperty request, Uri requestUri) {
			return new HttpRequestInfo(request, requestUri);
		}

		/// <summary>
		/// Creates an <see cref="HttpRequestBase"/> instance that describes the specified HTTP request.
		/// </summary>
		/// <param name="listenerRequest">The listener request.</param>
		/// <returns>An instance of <see cref="HttpRequestBase"/>.</returns>
		public static HttpRequestBase Create(HttpListenerRequest listenerRequest) {
			return new HttpRequestInfo(listenerRequest);
		}

#if CLR4
		/// <summary>
		/// Creates an <see cref="HttpRequestBase"/> instance that describes the specified HTTP request.
		/// </summary>
		/// <param name="request">The HTTP request.</param>
		/// <returns>An instance of <see cref="HttpRequestBase"/>.</returns>
		public static HttpRequestBase Create(HttpRequestMessage request) {
			return new HttpRequestInfo(request);
		}
#endif

		/// <summary>
		/// Creates an <see cref="HttpRequestBase"/> instance that describes the specified HTTP request.
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="form">The form variables.</param>
		/// <param name="headers">The HTTP headers.</param>
		/// <returns>An instance of <see cref="HttpRequestBase"/>.</returns>
		public static HttpRequestBase Create(string httpMethod, Uri requestUri, NameValueCollection form = null, NameValueCollection headers = null) {
			return new HttpRequestInfo(httpMethod, requestUri, form, headers);
		}

		/// <summary>
		/// Creates an <see cref="HttpRequestBase"/> instance that describes the specified HTTP request.
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="headers">The headers.</param>
		/// <param name="inputStream">The input stream.</param>
		/// <returns>An instance of <see cref="HttpRequestBase"/>.</returns>
		public static HttpRequestBase Create(string httpMethod, Uri requestUri, NameValueCollection headers, Stream inputStream) {
			return new HttpRequestInfo(httpMethod, requestUri, headers, inputStream);
		}

		/// <summary>
		/// Reads name=value pairs from the POSTed form entity when the HTTP headers indicate that that is the payload of the entity.
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <param name="headers">The headers.</param>
		/// <param name="inputStreamFunc">A function that returns the input stream.</param>
		/// <returns>The non-null collection of form variables.</returns>
		private static NameValueCollection ParseFormData(string httpMethod, NameValueCollection headers, Func<Stream> inputStreamFunc) {
			Requires.NotNullOrEmpty(httpMethod, "httpMethod");
			Requires.NotNull(headers, "headers");

			ContentType contentType = string.IsNullOrEmpty(headers[HttpRequestHeaders.ContentType]) ? null : new ContentType(headers[HttpRequestHeaders.ContentType]);
			if (httpMethod == "POST" && contentType != null && string.Equals(contentType.MediaType, Channel.HttpFormUrlEncoded, StringComparison.Ordinal) && inputStreamFunc != null) {
				var inputStream = inputStreamFunc();
				var reader = new StreamReader(inputStream);
				long originalPosition = 0;
				if (inputStream.CanSeek) {
					originalPosition = inputStream.Position;
				}
				string postEntity = reader.ReadToEnd();
				if (inputStream.CanSeek) {
					inputStream.Seek(originalPosition, SeekOrigin.Begin);
				}

				return HttpUtility.ParseQueryString(postEntity);
			}

			return new NameValueCollection();
		}

		/// <summary>
		/// Adds HTTP headers to a <see cref="NameValueCollection"/>.
		/// </summary>
		/// <param name="collectionToFill">The collection to be modified with added entries.</param>
		/// <param name="headers">The collection to read from.</param>
		private static void AddHeaders(NameValueCollection collectionToFill, HttpHeaders headers) {
			Requires.NotNull(collectionToFill, "collectionToFill");
			Requires.NotNull(headers, "headers");

			foreach (var header in headers) {
				foreach (var value in header.Value) {
					collectionToFill.Add(header.Key, value);
				}
			}
		}
	}
}
