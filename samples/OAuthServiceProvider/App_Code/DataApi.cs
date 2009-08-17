using System.Linq;
using System.ServiceModel;

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
public class DataApi : IDataApi {
	public int? GetAge() {
		return Global.LoggedInUser.Age;
	}

	public string GetName() {
		return Global.LoggedInUser.FullName;
	}

	public string[] GetFavoriteSites() {
		return Global.LoggedInUser.FavoriteSites.Select(site => site.SiteUrl).ToArray();
	}
}
