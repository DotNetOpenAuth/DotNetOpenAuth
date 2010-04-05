namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	public partial class AuthenticationToken {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationToken"/> class.
		/// </summary>
		public AuthenticationToken() {
			this.CreatedOnUtc = DateTime.UtcNow;
			this.LastUsedUtc = DateTime.UtcNow;
			this.UsageCount = 1;
		}

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

		partial void OnLastUsedUtcChanging(DateTime value) {
			Utilities.VerifyThrowNotLocalTime(value);
		}

		partial void OnCreatedOnUtcChanging(DateTime value) {
			Utilities.VerifyThrowNotLocalTime(value);
		}
	}
}
