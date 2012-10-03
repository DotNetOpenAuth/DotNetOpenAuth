﻿//-----------------------------------------------------------------------
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
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
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

			Reporting.RecordRequestStatistics(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo"/> class.
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <param name="requestUri">The request URI.</param>
		/// <param name="form">The form variables.</param>
		/// <param name="headers">The HTTP headers.</param>
		internal HttpRequestInfo(string httpMethod, Uri requestUri, NameValueCollection form = null, NameValueCollection headers = null) {
			Requires.NotNullOrEmpty(httpMethod, "httpMethod");
			Requires.NotNull(requestUri, "requestUri");

			this.httpMethod = httpMethod;
			this.requestUri = requestUri;
			this.form = form ?? new NameValueCollection();
			this.queryString = HttpUtility.ParseQueryString(requestUri.Query);
			this.headers = headers ?? new WebHeaderCollection();
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

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo"/> class.
		/// </summary>
		/// <param name="request">Details on the incoming HTTP request.</param>
		internal HttpRequestInfo(HttpRequestMessage request) {
			Requires.NotNull(request, "request");

			this.httpMethod = request.Method.Method;
			this.requestUri = request.RequestUri;
			this.queryString = HttpUtility.ParseQueryString(request.RequestUri.Query);
			this.headers = ReadHeaders(request);
			this.form = ParseFormData(request.Method.Method, this.headers, request.Content);
			this.serverVariables = new NameValueCollection();

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
			this.form = ParseFormData(httpMethod, headers, inputStream);
			this.serverVariables = new NameValueCollection();

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

		/// <summary>
		/// Creates an <see cref="HttpRequestBase"/> instance that describes the specified HTTP request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>An instance of <see cref="HttpRequestBase"/>.</returns>
		public static HttpRequestBase Create(HttpRequestMessage request) {
			return new HttpRequestInfo(request);
		}

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
		/// <param name="inputStream">The input stream.</param>
		/// <returns>The non-null collection of form variables.</returns>
		private static NameValueCollection ParseFormData(string httpMethod, NameValueCollection headers, Stream inputStream) {
			Requires.NotNullOrEmpty(httpMethod, "httpMethod");
			Requires.NotNull(headers, "headers");

			if (inputStream != null && ContentTypeIsHttpFormUrlEncoded(httpMethod, headers)) {
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
		/// Reads name=value pairs from the POSTed form entity when the HTTP headers indicate that that is the payload of the entity.
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <param name="headers">The headers.</param>
		/// <param name="content">The input content.</param>
		/// <returns>The non-null collection of form variables.</returns>
		private static NameValueCollection ParseFormData(string httpMethod, NameValueCollection headers, HttpContent content)
		{
			Requires.NotNullOrEmpty(httpMethod, "httpMethod");
			Requires.NotNull(headers, "headers");

			if (content != null && ContentTypeIsHttpFormUrlEncoded(httpMethod, headers)) {
				string postEntity = content.ReadAsStringAsync().Result;
				return HttpUtility.ParseQueryString(postEntity);
			}

			return new NameValueCollection();
		}

		/// <summary>
		/// Checks whether the request headers indicate a POSTed payload which is "application/x-www-form-urlencoded".
		/// </summary>
		/// <param name="httpMethod">The HTTP method.</param>
		/// <param name="headers">The headers.</param>
		/// <returns>True, if the request is "application/x-www-form-urlencoded", false otherwise.</returns>
		private static bool ContentTypeIsHttpFormUrlEncoded(string httpMethod, NameValueCollection headers) {
			ContentType contentType = string.IsNullOrEmpty(headers[HttpRequestHeaders.ContentType]) ? null : new ContentType(headers[HttpRequestHeaders.ContentType]);
			return httpMethod == "POST" && contentType != null && string.Equals(contentType.MediaType, Channel.HttpFormUrlEncoded, StringComparison.Ordinal);
		}

		/// <summary>
		/// Reads the request and content headers from the supplied HttpRequestMessage.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns>The headers.</returns>
		private static NameValueCollection ReadHeaders(HttpRequestMessage request) {
			var headers = new NameValueCollection();
			AddHeaders(headers, request.Headers);

			if (request.Content != null) {
				AddHeaders(headers, request.Content.Headers);
			}

			return headers;
		}

		/// <summary>
		/// Adds the headers from a HttpHeaders instance to the NameValueCollection.
		/// </summary>
		/// <param name="outputHeaders">The NameValueCollection to which the headers are added.</param>
		/// <param name="inputHeaders">The HttpHeaders from which the headers are read.</param>
		private static void AddHeaders(NameValueCollection outputHeaders, HttpHeaders inputHeaders) {
			if (inputHeaders != null) {
				foreach (var header in inputHeaders) {
					outputHeaders.Add(header.Key, header.Value.FirstOrDefault());
				}
			}
		}
	}
}
