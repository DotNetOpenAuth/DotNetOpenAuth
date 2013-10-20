//-----------------------------------------------------------------------
// <copyright file="MvcExtensions.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web.Mvc;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// DotNetOpenAuth extensions for ASP.NET MVC.
	/// </summary>
	public static class MvcExtensions {
		/// <summary>
		/// Wraps a response message as an MVC <see cref="ActionResult"/> so it can be conveniently returned from an MVC controller's action method.
		/// </summary>
		/// <param name="response">The response message.</param>
		/// <returns>An <see cref="ActionResult"/> instance.</returns>
		public static ActionResult AsActionResult(this HttpResponseMessage response) {
			Requires.NotNull(response, "response");
			return new HttpResponseMessageActionResult(response);
		}

		/// <summary>
		/// An MVC <see cref="ActionResult"/> that wraps an <see cref="HttpResponseMessage"/>
		/// </summary>
		private class HttpResponseMessageActionResult : ActionResult {
			/// <summary>
			/// The wrapped response.
			/// </summary>
			private readonly HttpResponseMessage response;

			/// <summary>
			/// Initializes a new instance of the <see cref="HttpResponseMessageActionResult"/> class.
			/// </summary>
			/// <param name="response">The response.</param>
			internal HttpResponseMessageActionResult(HttpResponseMessage response) {
				Requires.NotNull(response, "response");
				this.response = response;
			}

			/// <summary>
			/// Enables processing of the result of an action method by a custom type that inherits from the <see cref="T:System.Web.Mvc.ActionResult" /> class.
			/// </summary>
			/// <param name="context">The context in which the result is executed. The context information includes the controller, HTTP content, request context, and route data.</param>
			public override void ExecuteResult(ControllerContext context) {
				// Sadly, MVC doesn't support writing to the response stream asynchronously.
				this.response.SendAsync(context.HttpContext).GetAwaiter().GetResult();
			}
		}
	}
}
