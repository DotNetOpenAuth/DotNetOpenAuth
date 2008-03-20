using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Specialized;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// Represents a Provider's response to an OpenId authentication request.
	/// </summary>
	public interface IResponse {
		/// <summary>
		/// The HTTP status code that should accompany the response.
		/// </summary>
		HttpStatusCode Code { get; }
		/// <summary>
		/// The HTTP headers that should be added to the response.
		/// </summary>
		WebHeaderCollection Headers { get; }
		/// <summary>
		/// The body that should be sent as the response content.
		/// </summary>
		byte[] Body { get; }
		/// <summary>
		/// Sends the response to the browser.
		/// </summary>
		/// <remarks>
		/// This requires an ASP.NET hosted web site.
		/// </remarks>
		void Send();
	}
}
