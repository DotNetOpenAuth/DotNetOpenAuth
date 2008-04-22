using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Extensions.AttributeExchange {
	/// <summary>
	/// An individual attribute's value(s) as supplied by an OpenID Provider
	/// in response to a prior request by an OpenID Relying Party as part of
	/// a fetch request, or by a relying party as part of a store request.
	/// </summary>
	[Serializable]
	public class AttributeValues {
		/// <remarks>
		/// This is internal because web sites should be using the 
		/// <see cref="AttributeRequest.Respond"/> method to instantiate.
		/// </remarks>
		internal AttributeValues() { }

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
