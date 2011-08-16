namespace OAuthAuthorizationServer.Models {
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;

	public class LogOnModel {
		[Required]
		[DisplayName("OpenID")]
		public string UserSuppliedIdentifier { get; set; }

		[DisplayName("Remember me?")]
		public bool RememberMe { get; set; }
	}
}
