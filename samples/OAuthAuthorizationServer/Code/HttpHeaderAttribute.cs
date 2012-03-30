namespace OAuthAuthorizationServer.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;

	/// <summary>
	/// Represents an attribute that is used to add HTTP Headers to a Controller Action response.
	/// </summary>
	public class HttpHeaderAttribute : ActionFilterAttribute {
		/// <summary>
		/// Initializes a new instance of the <see cref="HttpHeaderAttribute"/> class.
		/// </summary>
		/// <param name="name">The HTTP header name.</param>
		/// <param name="value">The HTTP header value.</param>
		public HttpHeaderAttribute(string name, string value) {
			this.Name = name;
			this.Value = value;
		}

		/// <summary>
		/// Gets or sets the name of the HTTP Header.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the value of the HTTP Header.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Called by the MVC framework after the action result executes.
		/// </summary>
		/// <param name="filterContext">The filter context.</param>
		public override void OnResultExecuted(ResultExecutedContext filterContext) {
			filterContext.HttpContext.Response.AppendHeader(this.Name, this.Value);
			base.OnResultExecuted(filterContext);
		}
	}
}