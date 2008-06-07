using System.Web;
using DotNetOpenId.Extensions.SimpleRegistration;

/// <summary>
/// Strong-typed bag of session state.
/// </summary>
public class State {
	public static ClaimsResponse ProfileFields {
		get { return HttpContext.Current.Session["ProfileFields"] as ClaimsResponse; }
		set { HttpContext.Current.Session["ProfileFields"] = value; }
	}
}
