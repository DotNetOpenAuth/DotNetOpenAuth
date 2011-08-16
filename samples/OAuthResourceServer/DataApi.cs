namespace OAuthResourceServer {
	using System.Linq;
	using System.Security.Principal;
	using System.ServiceModel;

	using Code;

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
		private IIdentity User {
			get { return OperationContext.Current.ServiceSecurityContext.PrimaryIdentity; }
		}

		public int? GetAge() {
			// We'll just make up an age personalized to the user by counting the length of the username.
			return this.User.Name.Length;
		}

		public string GetName() {
			return this.User.Name;
		}

		public string[] GetFavoriteSites() {
			// Just return a hard-coded list, to avoid having to have a database in a sample.
			return new string[] {
				"http://www.dotnetopenauth.net/",
				"http://www.oauth.net/",
				"http://www.openid.net/",
			};
		}
	}
}