namespace OpenIdProviderMvc.Code {
	using System;
	using System.Web.Security;
	using DotNetOpenAuth.ApplicationBlock.Provider;
	using DotNetOpenAuth.OpenId;
	using OpenIdProviderMvc.Models;

	internal class AnonymousIdentifierProvider : AnonymousIdentifierProviderBase {
		internal AnonymousIdentifierProvider()
			: base(Util.GetAppPathRootedUri("anon?id=")) {
		}

		protected override byte[] GetHashSaltForLocalIdentifier(Identifier localIdentifier) {
			// This is just a sample with no database... a real web app MUST return 
			// a reasonable salt here and have that salt be persistent for each user.
			var membership = (ReadOnlyXmlMembershipProvider)Membership.Provider;
			string username = User.GetUserFromClaimedIdentifier(new Uri(localIdentifier));
			string salt = membership.GetSalt(username);
			return Convert.FromBase64String(salt);
			////return AnonymousIdentifierProviderBase.GetNewSalt(5);
		}
	}
}
