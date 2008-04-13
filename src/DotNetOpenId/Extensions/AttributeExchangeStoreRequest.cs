using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// The Attribute Exchange Store message, request leg.
	/// </summary>
	public struct AttributeExchangeStoreRequest {
		/// <summary>
		/// Adds the properties of this Attribute Exchange request to an outgoing
		/// OpenID authentication request.
		/// </summary>
		public void AddToRequest(Provider.IAuthenticationRequest authenticationRequest) {
			throw new NotImplementedException();
		}
		/// <summary>
		/// Reads an incoming authentication request (from a relying party)
		/// for Attribute Exchange properties and returns an instance of this 
		/// struct with them.
		/// </summary>
		public static AttributeExchangeStoreRequest ReadFromRequest(IAuthenticationResponse response) {
			throw new NotImplementedException();
		}
	}
}
