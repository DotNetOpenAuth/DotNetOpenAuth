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
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Net.Mime;
	using System.ServiceModel.Channels;
	using System.Web;

	/// <summary>
	/// A property store of details of an incoming HTTP request.
	/// </summary>
	/// <remarks>
	/// This serves a very similar purpose to <see cref="HttpRequest"/>, except that
	/// ASP.NET does not let us fully initialize that class, so we have to write one
	/// of our one.
	/// </remarks>
	public class HttpRequestInfo : HttpRequestBase {
		private readonly string httpMethod;

		private readonly Uri requestUri;

		private readonly NameValueCollection queryString;

		private readonly NameValueCollection headers;

		private readonly NameValueCollection form;

		private readonly NameValueCollection serverVariables;

		internal HttpRequestInfo(HttpRequestMessageProperty request, Uri requestUri) {
			Requires.NotNull(request, "request");
			Requires.NotNull(requestUri, "requestUri");

			this.httpMethod = request.Method;
			this.headers = request.Headers;
			this.requestUri = requestUri;
			this.form = new NameValueCollection();
			this.serverVariables = new NameValueCollection();

			Reporting.RecordRequestStatistics(this);
		}

		internal HttpRequestInfo(string httpMethod, Uri requestUri, NameValueCollection form = null, NameValueCollection headers = null) {
			Requires.NotNullOrEmpty(httpMethod, "httpMethod");
			Requires.NotNull(requestUri, "requestUri");

			this.httpMethod = httpMethod;
			this.requestUri = requestUri;
			this.form = form ?? new NameValueCollection();
			this.queryString = HttpUtility.ParseQueryString(requestUri.Query);
			this.headers = headers ?? new NameValueCollection();
			this.serverVariables = new NameValueCollection();
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
			this.form = ParseFormData(listenerRequest.HttpMethod, listenerRequest.Headers, listenerRequest.InputStream);
			this.serverVariables = new NameValueCollection();

			Reporting.RecordRequestStatistics(this);
		}

		internal HttpRequestInfo(string httpMethod, Uri requestUri, NameValueCollection headers, Stream inputStream) {
			Requires.NotNullOrEmpty(httpMethod, "httpMethod");
			Requires.NotNull(requestUri, "requestUri");

			this.httpMethod = httpMethod;
			this.requestUri = requestUri;
			this.headers = headers;
			this.queryString = HttpUtility.ParseQueryString(requestUri.Query);
			this.form = ParseFormData(httpMethod, headers, inputStream);
			this.serverVariables = new NameValueCollection();

			Reporting.RecordRequestStatistics(this);
		}

		public override string HttpMethod {
			get { return this.httpMethod; }
		}

		public override NameValueCollection Headers {
			get { return this.headers; }
		}

		public override Uri Url {
			get { return this.requestUri; }
		}

		public override string RawUrl {
			get { return this.requestUri.AbsolutePath + this.requestUri.Query; }
		}

		public override NameValueCollection Form {
			get { return this.form; }
		}

		public override NameValueCollection QueryString {
			get { return this.queryString; }
		}

		public override NameValueCollection ServerVariables {
			get { return this.serverVariables; }
		}

		public static HttpRequestBase Create(HttpRequestMessageProperty request, Uri requestUri) {
			return new HttpRequestInfo(request, requestUri);
		}

		public static HttpRequestBase Create(HttpListenerRequest listenerRequest) {
			return new HttpRequestInfo(listenerRequest);
		}

		public static HttpRequestBase Create(string httpMethod, Uri requestUri, NameValueCollection form = null, NameValueCollection headers = null) {
			return new HttpRequestInfo(httpMethod, requestUri, form, headers);
		}

		public static HttpRequestBase Create(string httpMethod, Uri requestUri, NameValueCollection headers, Stream inputStream) {
			return new HttpRequestInfo(httpMethod, requestUri, headers, inputStream);
		}

		private static NameValueCollection ParseFormData(string httpMethod, NameValueCollection headers, Stream inputStream) {
			Requires.NotNullOrEmpty(httpMethod, "httpMethod");
			Requires.NotNull(headers, "headers");

			ContentType contentType = string.IsNullOrEmpty(headers[HttpRequestHeaders.ContentType]) ? null : new ContentType(headers[HttpRequestHeaders.ContentType]);
			if (inputStream != null && httpMethod == "POST" && contentType != null && string.Equals(contentType.MediaType, Channel.HttpFormUrlEncoded, StringComparison.Ordinal)) {
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
	}
}
