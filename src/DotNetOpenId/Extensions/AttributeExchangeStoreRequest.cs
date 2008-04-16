using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;
using System.Globalization;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// The Attribute Exchange Store message, request leg.
	/// </summary>
	public class AttributeExchangeStoreRequest : IExtensionRequest {
		readonly string Mode = "store_request";

		/// <summary>
		/// Reads an incoming authentication request (from a relying party)
		/// for Attribute Exchange properties and returns an instance of this 
		/// struct with them.
		/// </summary>
		public static AttributeExchangeStoreRequest ReadFromRequest(Provider.IRequest request) {
			var obj = new AttributeExchangeStoreRequest();
			return ((IExtensionRequest)obj).ReadFromRequest(request) ? obj : null;
		}

		#region IExtensionRequest Members
		string IExtensionRequest.TypeUri { get { return Constants.ae.ns; } }

		public void AddToRequest(RelyingParty.IAuthenticationRequest authenticationRequest) {
			var fields = new Dictionary<string, string> {
				{ "mode", Mode },
			};
			authenticationRequest.AddExtensionArguments(Constants.ae.ns, fields);
		}

		bool IExtensionRequest.ReadFromRequest(DotNetOpenId.Provider.IRequest request) {
			var fields = request.GetExtensionArguments(Constants.ae.ns);
			if (fields == null) return false;
			string mode;
			fields.TryGetValue("mode", out mode);
			if (mode != Mode) return false;

			return true;
		}

		#endregion
	}
}
