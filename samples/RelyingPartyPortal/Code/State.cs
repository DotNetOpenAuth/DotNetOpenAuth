using System.Web;
using DotNetOpenId.Extensions.SimpleRegistration;
using System.Collections.Generic;
using DotNetOpenId.Extensions.ProviderAuthenticationPolicy;

/// <summary>
/// Strong-typed bag of session state.
/// </summary>
public class State {
	public static void Clear() {
		ProfileFields = null;
		FriendlyLoginName = null;
		PapePolicies = null;
	}
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
}
