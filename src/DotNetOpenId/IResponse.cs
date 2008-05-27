using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Specialized;

namespace DotNetOpenId {
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		byte[] Body { get; }
		/// <summary>
		/// Sends the response to the browser.
		/// </summary>
		/// <remarks>
		/// This requires an ASP.NET HttpContext.
		/// </remarks>
		void Send();
	}
}
