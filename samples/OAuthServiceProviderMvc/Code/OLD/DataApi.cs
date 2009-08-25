using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;

/// <summary>
/// The WCF service API.
/// </summary>
/// <remarks>
/// Note how there is no code here that is bound to OAuth or any other
/// credential/authorization scheme.  That's all part of the channel/binding elsewhere.
/// And the reference to Global.LoggedInUser is the user being impersonated by the WCF client.
/// In the OAuth case, it is the user who authorized the OAuth access token that was used
/// to gain access to the service.
/// </remarks>
[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
public class DataApi : IDataApi {
	public int? GetAge() {
        if (!OAuthAuthorizationManager.CheckAccess()) return null;

		return Global.LoggedInUser.Age;
	}

	public string GetName() {
        if (!OAuthAuthorizationManager.CheckAccess()) return null;

		return Global.LoggedInUser.FullName;
	}

	public string[] GetFavoriteSites() {
        if (!OAuthAuthorizationManager.CheckAccess()) return null;

		return Global.LoggedInUser.FavoriteSites.Select(site => site.SiteUrl).ToArray();
	}
}
