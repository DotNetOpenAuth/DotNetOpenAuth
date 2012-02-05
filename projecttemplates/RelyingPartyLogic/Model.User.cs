//-----------------------------------------------------------------------
// <copyright file="Model.User.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.IdentityModel.Claims;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.InfoCard;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public partial class User {
		/// <summary>
		/// Initializes a new instance of the <see cref="User"/> class.
		/// </summary>
		public User() {
			this.CreatedOnUtc = DateTime.UtcNow;
		}

		public static AuthenticationToken ProcessUserLogin(IAuthenticationResponse openIdResponse) {
			bool trustedEmail = Policies.ProviderEndpointsProvidingTrustedEmails.Contains(openIdResponse.Provider.Uri);
			return ProcessUserLogin(openIdResponse.ClaimedIdentifier, openIdResponse.FriendlyIdentifierForDisplay, openIdResponse.GetExtension<ClaimsResponse>(), null, trustedEmail);
		}

		public static AuthenticationToken ProcessUserLogin(Token samlToken) {
			bool trustedEmail = false; // we don't trust InfoCard email addresses, since these can be self-issued.
			return ProcessUserLogin(
				AuthenticationToken.SynthesizeClaimedIdentifierFromInfoCard(samlToken.UniqueId),
				samlToken.SiteSpecificId,
				null,
				samlToken,
				trustedEmail);
		}

		private static AuthenticationToken ProcessUserLogin(string claimedIdentifier, string friendlyIdentifier, ClaimsResponse claims, Token samlToken, bool trustedEmail) {
			// Create an account for this user if we don't already have one.
			AuthenticationToken openidToken = Database.DataContext.AuthenticationTokens.FirstOrDefault(token => token.ClaimedIdentifier == claimedIdentifier);
			if (openidToken == null) {
				// this is a user we haven't seen before.
				User user = new User();
				openidToken = new AuthenticationToken {
					ClaimedIdentifier = claimedIdentifier,
					FriendlyIdentifier = friendlyIdentifier,
				};
				user.AuthenticationTokens.Add(openidToken);

				// Gather information about the user if it's available.
				if (claims != null) {
					if (!string.IsNullOrEmpty(claims.Email)) {
						user.EmailAddress = claims.Email;
						user.EmailAddressVerified = trustedEmail;
					}
					if (!string.IsNullOrEmpty(claims.FullName)) {
						if (claims.FullName.IndexOf(' ') > 0) {
							user.FirstName = claims.FullName.Substring(0, claims.FullName.IndexOf(' ')).Trim();
							user.LastName = claims.FullName.Substring(claims.FullName.IndexOf(' ')).Trim();
						} else {
							user.FirstName = claims.FullName;
						}
					}
				} else if (samlToken != null) {
					string email, givenName, surname;
					if (samlToken.Claims.TryGetValue(ClaimTypes.Email, out email)) {
						user.EmailAddress = email;
						user.EmailAddressVerified = trustedEmail;
					}
					if (samlToken.Claims.TryGetValue(ClaimTypes.GivenName, out givenName)) {
						user.FirstName = givenName;
					}
					if (samlToken.Claims.TryGetValue(ClaimTypes.Surname, out surname)) {
						user.LastName = surname;
					}
				}

				Database.DataContext.AddToUsers(user);
			} else {
				openidToken.UsageCount++;
				openidToken.LastUsedUtc = DateTime.UtcNow;
			}
			return openidToken;
		}

		partial void OnCreatedOnUtcChanging(DateTime value) {
			Utilities.VerifyThrowNotLocalTime(value);
		}

		partial void OnEmailAddressChanged() {
			// Whenever the email address is changed, we must reset its verified status.
			this.EmailAddressVerified = false;
		}
	}
}
