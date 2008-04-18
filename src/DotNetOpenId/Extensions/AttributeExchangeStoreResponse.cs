using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.RelyingParty;
using System.Globalization;
using System.Diagnostics;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// The Attribute Exchange Store message, response leg.
	/// </summary>
	public class AttributeExchangeStoreResponse : IExtensionResponse {
		const string SuccessMode = "store_response_success";
		const string FailureMode = "store_response_failure";

		/// <summary>
		/// Whether the storage request succeeded.
		/// </summary>
		public bool Succeeded { get; set; }
		/// <summary>
		/// The reason for the failure.
		/// </summary>
		public string FailureReason { get; set; }

		/// <summary>
		/// Reads a Provider's response for Attribute Exchange values and returns
		/// an instance of this struct with the values.
		/// </summary>
		public static AttributeExchangeStoreResponse ReadFromResponse(IAuthenticationResponse response) {
			var obj = new AttributeExchangeStoreResponse();
			return ((IExtensionResponse)obj).ReadFromResponse(response) ? obj : null;
		}

		#region IExtensionResponse Members
		string IExtensionResponse.TypeUri { get { return Constants.ae.ns; } }

		public void AddToResponse(Provider.IRequest authenticationRequest) {
			var fields = new Dictionary<string, string> {
				{ "mode", Succeeded ? SuccessMode : FailureMode },
			};
			if (!Succeeded && !string.IsNullOrEmpty(FailureReason))
				fields.Add("error", FailureReason);

			authenticationRequest.AddExtensionArguments(Constants.ae.ns, fields);
		}

		bool IExtensionResponse.ReadFromResponse(IAuthenticationResponse response) {
			var fields = response.GetExtensionArguments(Constants.ae.ns);
			if (fields == null) return false;
			string mode;
			if (!fields.TryGetValue("mode", out mode)) return false;
			switch (mode) {
				case SuccessMode:
					Succeeded = true;
					break;
				case FailureMode:
					Succeeded = false;
					break;
				default:
					return false;
			}

			if (!Succeeded) {
				string error;
				if (fields.TryGetValue("error", out error))
					FailureReason = error;
			}

			return true;
		}

		#endregion
	}
}
