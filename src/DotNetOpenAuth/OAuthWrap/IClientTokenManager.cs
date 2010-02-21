//-----------------------------------------------------------------------
// <copyright file="IClientTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
using System.Diagnostics.Contracts;

	[ContractClass(typeof(IClientTokenManagerContract))]
	public interface IClientTokenManager {
		IWrapAuthorization GetAuthorizationState(Uri callbackUrl, string clientState);
	}

	[ContractClassFor(typeof(IClientTokenManager))]
	internal abstract class IClientTokenManagerContract : IClientTokenManager {
		private IClientTokenManagerContract() {
		}

		#region IClientTokenManager Members

		IWrapAuthorization IClientTokenManager.GetAuthorizationState(Uri callbackUrl, string clientState) {
			Contract.Requires<ArgumentNullException>(callbackUrl != null);
			throw new NotImplementedException();
		}

		#endregion
	}

	public interface IWrapAuthorization {
		Uri Callback { get; set; }
		string RefreshToken { get; set; }
		string AccessToken { get; set; }
		DateTime? AccessTokenExpirationUtc { get; set; }
		string Scope { get; set; }

		void Delete();
		void SaveChanges();
	}
}
