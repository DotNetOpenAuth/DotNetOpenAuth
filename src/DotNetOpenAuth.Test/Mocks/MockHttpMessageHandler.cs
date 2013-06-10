//-----------------------------------------------------------------------
// <copyright file="MockHttpMessageHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Validation;

	/// <summary>
	/// An <see cref="HttpMessageHandler"/> that sends each request to the specified delegate.
	/// </summary>
	internal class MockHttpMessageHandler : HttpMessageHandler {
		/// <summary>
		/// The handler to invoke for each request.
		/// </summary>
		private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler;

		/// <summary>
		/// Initializes a new instance of the <see cref="MockHttpMessageHandler" /> class.
		/// </summary>
		/// <param name="handler">The handler.</param>
		internal MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) {
			Requires.NotNull(handler, "handler");
			this.handler = handler;
		}

		/// <summary>
		/// Send an HTTP request as an asynchronous operation.
		/// </summary>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="cancellationToken">The cancellation token to cancel operation.</param>
		/// <returns>
		/// Returns <see cref="T:System.Threading.Tasks.Task`1" />.The task object representing the asynchronous operation.
		/// </returns>
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			return this.handler(request, cancellationToken);
		}
	}
}
