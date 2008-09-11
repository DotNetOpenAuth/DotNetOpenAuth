using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Extensions {
	internal class ExtensionManager {
		/// <summary>
		/// A list of request extensions that may be enumerated over for logging purposes.
		/// </summary>
		internal static Dictionary<IExtensionRequest, string> RequestExtensions = new Dictionary<IExtensionRequest, string> {
			{new AttributeExchange.FetchRequest(), "AX fetch"},
			{new AttributeExchange.StoreRequest(), "AX store"},
			{new ProviderAuthenticationPolicy.PolicyRequest(), "PAPE"},
			{new SimpleRegistration.ClaimsRequest(), "sreg"},
		};
		//internal static List<IExtensionResponse> ResponseExtensions = new List<IExtensionResponse> {
		//    new AttributeExchange.FetchResponse(),
		//    new AttributeExchange.StoreResponse(),
		//    new ProviderAuthenticationPolicy.PolicyResponse(),
		//    new SimpleRegistration.ClaimsResponse(),
		//};
	}
}
