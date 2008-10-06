using System.Linq;
using System.ServiceModel;

public class DataApi : IDataApi {
	private static OAuthToken AccessToken {
		get { return OperationContext.Current.IncomingMessageProperties["OAuthAccessToken"] as OAuthToken; }
	}

	public int? GetAge() {
		return AccessToken.User.Age;
	}

	public string GetName() {
		return AccessToken.User.FullName;
	}

	public string[] GetFavoriteSites() {
		return AccessToken.User.FavoriteSites.Select(site => site.SiteUrl).ToArray();
	}
}
