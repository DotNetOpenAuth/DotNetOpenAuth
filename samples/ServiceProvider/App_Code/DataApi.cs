using System.Linq;
using System.Globalization;
using System.ServiceModel;
using System.Text;

public class DataApi : IDataApi {
	public int? GetAge() {
		return AccessToken.User.Age;
	}

	public string GetName() {
		return AccessToken.User.FullName;
	}

	public string[] GetFavoriteSites() {
		return AccessToken.User.FavoriteSites.Select(site => site.SiteUrl).ToArray();
	}

	private static OAuthToken AccessToken {
		get { return OperationContext.Current.IncomingMessageProperties["OAuthAccessToken"] as OAuthToken; }
	}
}
