using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Extensions.AuthenticationPolicy {
	public class PolicyResponse : IExtensionResponse {
		#region IExtensionResponse Members

		IDictionary<string, string> IExtensionResponse.Serialize(DotNetOpenId.Provider.IRequest authenticationRequest) {
			throw new NotImplementedException();
		}

		bool IExtensionResponse.Deserialize(IDictionary<string, string> fields, DotNetOpenId.RelyingParty.IAuthenticationResponse response) {
			throw new NotImplementedException();
		}

		#endregion

		#region IExtension Members

		string IExtension.TypeUri {
			get { return Constants.TypeUri; }
		}

		#endregion
	}
}
