//-----------------------------------------------------------------------
// <copyright file="MvcExtensions.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web.Mvc;

	/// <summary>
	/// Extensions for ASP.NET MVC.
	/// </summary>
	public static class MvcExtensions {
		/// <summary>
		/// Transforms an OutgoingWebResponse to an MVC-friendly ActionResult.
		/// </summary>
		/// <param name="response">The response to send to the user agent.</param>
		/// <returns>The <see cref="ActionResult"/> instance to be returned by the Controller's action method.</returns>
		public static ActionResult AsActionResult(this OutgoingWebResponse response) {
			Requires.NotNull(response, "response");
			return new OutgoingWebResponseActionResult(response);
		}
	}
}
