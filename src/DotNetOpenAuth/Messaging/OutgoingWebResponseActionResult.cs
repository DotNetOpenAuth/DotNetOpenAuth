//-----------------------------------------------------------------------
// <copyright file="OutgoingWebResponseActionResult.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.Diagnostics.Contracts;
	using System.Web.Mvc;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An ASP.NET MVC structure to represent the response to send
	/// to the user agent when the controller has finished its work.
	/// </summary>
	internal class OutgoingWebResponseActionResult : ActionResult {
		/// <summary>
		/// The outgoing web response to send when the ActionResult is executed.
		/// </summary>
		private readonly OutgoingWebResponse response;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutgoingWebResponseActionResult"/> class.
		/// </summary>
		/// <param name="response">The response.</param>
		internal OutgoingWebResponseActionResult(OutgoingWebResponse response) {
			Contract.Requires(response != null);
			ErrorUtilities.VerifyArgumentNotNull(response, "response");
			this.response = response;
		}

		/// <summary>
		/// Enables processing of the result of an action method by a custom type that inherits from <see cref="T:System.Web.Mvc.ActionResult"/>.
		/// </summary>
		/// <param name="context">The context in which to set the response.</param>
		public override void ExecuteResult(ControllerContext context) {
			this.response.Send();
		}
	}
}
