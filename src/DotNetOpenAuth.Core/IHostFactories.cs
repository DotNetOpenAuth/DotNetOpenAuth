//-----------------------------------------------------------------------
// <copyright file="IHostFactories.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Provides the host application or tests with the ability to create standard message handlers or stubs.
	/// </summary>
	public interface IHostFactories {
		/// <summary>
		/// Initializes a new instance of a concrete derivation of <see cref="HttpMessageHandler"/> 
		/// to be used for outbound HTTP traffic.
		/// </summary>
		/// <returns>An instance of <see cref="HttpMessageHandler"/>.</returns>
		/// <remarks>
		/// An instance of <see cref="WebRequestHandler"/> is recommended where available;
		/// otherwise an instance of <see cref="HttpClientHandler"/> is recommended.
		/// </remarks>
		HttpMessageHandler CreateHttpMessageHandler();

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClient"/> class
		/// to be used for outbound HTTP traffic.
		/// </summary>
		/// <param name="handler">
		/// The handler to pass to the <see cref="HttpClient"/> constructor.
		/// May be null to use the default that would be provided by <see cref="CreateHttpMessageHandler"/>.
		/// </param>
		/// <returns>An instance of <see cref="HttpClient"/>.</returns>
		HttpClient CreateHttpClient(HttpMessageHandler handler = null);
	}
}
