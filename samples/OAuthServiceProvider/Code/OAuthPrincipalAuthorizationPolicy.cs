﻿namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.IdentityModel.Claims;
	using System.IdentityModel.Policy;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public class OAuthPrincipalAuthorizationPolicy : IAuthorizationPolicy {
		private readonly Guid uniqueId = Guid.NewGuid();
		private readonly OAuthPrincipal principal;

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthPrincipalAuthorizationPolicy"/> class.
		/// </summary>
		/// <param name="principal">The principal.</param>
		public OAuthPrincipalAuthorizationPolicy(OAuthPrincipal principal) {
			this.principal = principal;
		}

		#region IAuthorizationComponent Members

		/// <summary>
		/// Gets a unique ID for this instance.
		/// </summary>
		public string Id {
			get { return this.uniqueId.ToString(); }
		}

		#endregion

		#region IAuthorizationPolicy Members

		public ClaimSet Issuer {
			get { return ClaimSet.System; }
		}

		public bool Evaluate(EvaluationContext evaluationContext, ref object state) {
			evaluationContext.AddClaimSet(this, new DefaultClaimSet(Claim.CreateNameClaim(this.principal.Identity.Name)));
			evaluationContext.Properties["Principal"] = this.principal;
			return true;
		}

		#endregion
	}
}