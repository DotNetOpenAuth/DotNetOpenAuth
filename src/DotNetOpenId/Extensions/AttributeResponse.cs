using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// An individual attribute's value(s) as supplied by an OpenID Provider
	/// in response to a prior request by an OpenID Relying Party.
	/// </summary>
	public class AttributeResponse {
		internal AttributeResponse() { }

		/// <summary>
		/// The URI uniquely identifying the attribute whose value is being supplied.
		/// </summary>
		public string TypeUri { get; internal set; }

		/// <summary>
		/// Gets the values supplied by the Provider.
		/// </summary>
		public string[] Values;
	}
}
