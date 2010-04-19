namespace OAuthServiceProvider {
	using System.Linq;
	using System.ServiceModel;
	using OAuthServiceProvider.Code;

	/// <summary>
	/// The WCF service API.
	/// </summary>
	/// <remarks>
	/// Note how there is no code here that is bound to OAuth or any other
	/// credential/authorization scheme.  That's all part of the channel/binding elsewhere.
	/// And the reference to OperationContext.Current.ServiceSecurityContext.PrimaryIdentity 
	/// is the user being impersonated by the WCF client.
	/// In the OAuth case, it is the user who authorized the OAuth access token that was used
	/// to gain access to the service.
	/// </remarks>
	public class DataApi : IDataApi {
		private User User {
			get { return OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.GetUser(); }
		}

		public int? GetAge() {
			return User.Age;
		}

		public string GetName() {
			return User.FullName;
		}

		public string[] GetFavoriteSites() {
			return User.FavoriteSites.Select(site => site.SiteUrl).ToArray();
		}
	}
}