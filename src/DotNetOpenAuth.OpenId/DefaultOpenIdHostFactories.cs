//-----------------------------------------------------------------------
// <copyright file="DefaultOpenIdHostFactories.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Cache;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Creates default instances of required dependencies.
	/// </summary>
	public class DefaultOpenIdHostFactories : IHostFactories {
		/// <summary>
		/// Initializes a new instance of a concrete derivation of <see cref="HttpMessageHandler" />
		/// to be used for outbound HTTP traffic.
		/// </summary>
		/// <returns>An instance of <see cref="HttpMessageHandler"/>.</returns>
		/// <remarks>
		/// An instance of <see cref="WebRequestHandler" /> is recommended where available;
		/// otherwise an instance of <see cref="HttpClientHandler" /> is recommended.
		/// </remarks>
		public virtual HttpMessageHandler CreateHttpMessageHandler() {
			var handler = new UntrustedWebRequestHandler();
			((WebRequestHandler)handler.InnerHandler).CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
			return handler;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClient" /> class
		/// to be used for outbound HTTP traffic.
		/// </summary>
		/// <param name="handler">The handler to pass to the <see cref="HttpClient" /> constructor.
		/// May be null to use the default that would be provided by <see cref="CreateHttpMessageHandler" />.</param>
		/// <returns>
		/// An instance of <see cref="HttpClient" />.
		/// </returns>
		public HttpClient CreateHttpClient(HttpMessageHandler handler) {
			handler = handler ?? this.CreateHttpMessageHandler();
			var untrustedHandler = handler as UntrustedWebRequestHandler;
			HttpClient client;
			if (untrustedHandler != null) {
				client = untrustedHandler.CreateClient();
			} else {
				client = new HttpClient(handler);
			}

			client.DefaultRequestHeaders.UserAgent.Add(Util.LibraryVersionHeader);
			return client;
		}
	}
}
