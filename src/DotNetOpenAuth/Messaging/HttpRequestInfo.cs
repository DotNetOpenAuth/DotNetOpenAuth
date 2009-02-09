//-----------------------------------------------------------------------
// <copyright file="HttpRequestInfo.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.IO;
	using System.Net;
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
	public class HttpRequestInfo {
		/// <summary>
		/// The key/value pairs found in the entity of a POST request.
		/// </summary>
		private NameValueCollection form;

		/// <summary>
		/// The key/value pairs found in the querystring of the incoming request.
		/// </summary>
		private NameValueCollection queryString;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo"/> class.
		/// </summary>
		internal HttpRequestInfo() {
			this.HttpMethod = "GET";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo"/> class.
		/// </summary>
		/// <param name="request">The ASP.NET structure to copy from.</param>
		internal HttpRequestInfo(HttpRequest request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			this.HttpMethod = request.HttpMethod;
			this.Url = request.Url;
			this.Headers = GetHeaderCollection(request.Headers);
			this.InputStream = request.InputStream;

			// These values would normally be calculated, but we'll reuse them from
			// HttpRequest since they're already calculated, and there's a chance (<g>)
			// that ASP.NET does a better job of being comprehensive about gathering
			// these as well.
			this.form = request.Form;
			this.queryString = request.QueryString;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo"/> class.
		/// </summary>
		/// <param name="request">The WCF incoming request structure to get the HTTP information from.</param>
		/// <param name="requestUri">The URI of the service endpoint.</param>
		internal HttpRequestInfo(HttpRequestMessageProperty request, Uri requestUri) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			this.HttpMethod = request.Method;
			this.Headers = request.Headers;
			this.Url = requestUri;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo"/> class.
		/// </summary>
		/// <param name="request">The HttpWebRequest (that was never used) to copy from.</param>
		internal HttpRequestInfo(WebRequest request) {
			this.HttpMethod = request.Method;
			this.Url = request.RequestUri;
			this.Headers = GetHeaderCollection(request.Headers);
			this.InputStream = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestInfo"/> class.
		/// </summary>
		/// <param name="message">The message being passed in through a mock transport.  May be null.</param>
		/// <param name="httpMethod">The HTTP method that the incoming request came in on, whether or not <paramref name="message"/> is null.</param>
		internal HttpRequestInfo(IDirectedProtocolMessage message, HttpDeliveryMethods httpMethod) {
			this.Message = message;
			if ((httpMethod & HttpDeliveryMethods.GetRequest) != 0) {
				this.HttpMethod = "GET";
			} else if ((httpMethod & HttpDeliveryMethods.PostRequest) != 0) {
				this.HttpMethod = "POST";
			}
		}

		/// <summary>
		/// Gets or sets the message that is being sent over a mock transport (for testing).
		/// </summary>
		internal IDirectedProtocolMessage Message { get; set; }

		/// <summary>
		/// Gets or sets the verb in the request (i.e. GET, POST, etc.)
		/// </summary>
		internal string HttpMethod { get; set; }

		/// <summary>
		/// Gets or sets the entire URL of the request.
		/// </summary>
		internal Uri Url { get; set; }

		/// <summary>
		/// Gets the query part of the URL (The ? and everything after it).
		/// </summary>
		internal string Query {
			get { return this.Url != null ? this.Url.Query : null; }
		}

		/// <summary>
		/// Gets or sets the collection of headers that came in with the request.
		/// </summary>
		internal WebHeaderCollection Headers { get; set; }

		/// <summary>
		/// Gets or sets the entity, or body of the request, if any.
		/// </summary>
		internal Stream InputStream { get; set; }

		/// <summary>
		/// Gets the key/value pairs found in the entity of a POST request.
		/// </summary>
		internal NameValueCollection Form {
			get {
				if (this.form == null) {
					if (this.HttpMethod == "POST" && this.Headers[HttpRequestHeader.ContentType] == "application/x-www-form-urlencoded") {
						StreamReader reader = new StreamReader(this.InputStream);
						long originalPosition = this.InputStream.Position;
						this.form = HttpUtility.ParseQueryString(reader.ReadToEnd());
						if (this.InputStream.CanSeek) {
							this.InputStream.Seek(originalPosition, SeekOrigin.Begin);
						}
					} else {
						this.form = new NameValueCollection();
					}
				}

				return this.form;
			}
		}

		/// <summary>
		/// Gets the key/value pairs found in the querystring of the incoming request.
		/// </summary>
		internal NameValueCollection QueryString {
			get {
				if (this.queryString == null) {
					this.queryString = this.Query != null ? HttpUtility.ParseQueryString(this.Query) : new NameValueCollection();
				}

				return this.queryString;
			}
		}

		/// <summary>
		/// Converts a NameValueCollection to a WebHeaderCollection.
		/// </summary>
		/// <param name="pairs">The collection a HTTP headers.</param>
		/// <returns>A new collection of the given headers.</returns>
		private static WebHeaderCollection GetHeaderCollection(NameValueCollection pairs) {
			Debug.Assert(pairs != null, "pairs == null");

			WebHeaderCollection headers = new WebHeaderCollection();
			foreach (string key in pairs) {
				headers.Add(key, pairs[key]);
			}

			return headers;
		}
	}
}
