namespace OpenIdRelyingPartyWebForms {
	using System.Collections.Generic;
	using System.Web;
	using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

	/// <summary>
	/// Strong-typed bag of session state.
	/// </summary>
	public class State {
		public static ClaimsResponse ProfileFields {
			get { return HttpContext.Current.Session["ProfileFields"] as ClaimsResponse; }
			set { HttpContext.Current.Session["ProfileFields"] = value; }
		}

		public static string FriendlyLoginName {
			get { return HttpContext.Current.Session["FriendlyUsername"] as string; }
			set { HttpContext.Current.Session["FriendlyUsername"] = value; }
		}

		public static PolicyResponse PapePolicies {
			get { return HttpContext.Current.Session["PapePolicies"] as PolicyResponse; }
			set { HttpContext.Current.Session["PapePolicies"] = value; }
		}

		public static void Clear() {
			ProfileFields = null;
			FriendlyLoginName = null;
			PapePolicies = null;
		}
	}
}