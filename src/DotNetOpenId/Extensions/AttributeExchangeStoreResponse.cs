using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// The Attribute Exchange Store message, response leg.
	/// </summary>
	public struct AttributeExchangeStoreResponse {
		/// <summary>
		/// Adds the values of this struct to an authentication response being prepared
		/// by an OpenID Provider.
		/// </summary>
		public void AddToResponse(Provider.IAuthenticationRequest authenticationRequest) {
			throw new NotImplementedException();
		}
		/// <summary>
		/// Reads a Provider's response for Attribute Exchange values and returns
		/// an instance of this struct with the values.
		/// </summary>
		public static AttributeExchangeStoreResponse ReadFromResponse(IAuthenticationResponse response) {
			throw new NotImplementedException();
		}
	}
}
