namespace OpenIdProviderMvc.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId;
	using System.Web.Security;
	using OpenIdProviderMvc.Models;
	using DotNetOpenAuth.ApplicationBlock.Provider;

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
