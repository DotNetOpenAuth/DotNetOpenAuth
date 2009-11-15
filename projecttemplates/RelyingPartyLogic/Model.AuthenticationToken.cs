namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	public partial class AuthenticationToken {
		public bool IsInfoCard {
			get { return this.ClaimedIdentifier.StartsWith(UriPrefixForInfoCard); }
		}

		private static string UriPrefixForInfoCard {
			get { return new Uri(Utilities.ApplicationRoot, "infocard/").AbsoluteUri; }
		}

		public static string SynthesizeClaimedIdentifierFromInfoCard(string uniqueId) {
			string synthesizedClaimedId = UriPrefixForInfoCard + Uri.EscapeDataString(uniqueId);
			return synthesizedClaimedId;
		}
	}
}
