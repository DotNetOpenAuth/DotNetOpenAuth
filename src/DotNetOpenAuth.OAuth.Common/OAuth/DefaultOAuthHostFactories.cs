//-----------------------------------------------------------------------
// <copyright file="DefaultOAuthHostFactories.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Net.Http;

	/// <summary>
	/// Creates default instances of required dependencies.
	/// </summary>
	public class DefaultOAuthHostFactories : IHostFactories {
		/// <summary>
		/// Initializes a new instance of a concrete derivation of <see cref="HttpMessageHandler" />
		/// to be used for outbound HTTP traffic.
		/// </summary>
		/// <returns>An instance of <see cref="HttpMessageHandler"/>.</returns>
		/// <remarks>
		/// An instance of WebRequestHandler is recommended where available;
		/// otherwise an instance of <see cref="HttpClientHandler" /> is recommended.
		/// </remarks>
		public virtual HttpMessageHandler CreateHttpMessageHandler() {
			var handler = new HttpClientHandler();
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
			var client = new HttpClient(handler);
			client.DefaultRequestHeaders.UserAgent.Add(PortableUtilities.LibraryVersionHeader);
			return client;
		}
	}
}
