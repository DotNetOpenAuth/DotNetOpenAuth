using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Extensions.AttributeExchange {
	/// <summary>
	/// Indicates a relying party's level of desire for a particular value
	/// to be provided by the OpenID Provider.
	/// </summary>
	public enum DemandLevel {
		/// <summary>
		/// The relying party considers this information as optional.
		/// </summary>
		Optional,
		/// <summary>
		/// The relying party considers this information as required.
		/// Note however, that the Provider still has the option to not supply this value.
		/// </summary>
		Required,
	}
}
