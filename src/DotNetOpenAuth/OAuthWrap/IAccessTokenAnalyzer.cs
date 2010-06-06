//-----------------------------------------------------------------------
// <copyright file="IAccessTokenAnalyzer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	public interface IAccessTokenAnalyzer {
		bool TryValidateAccessToken(IDirectedProtocolMessage message, string accessToken, out string user, out string scope);
	}

	internal abstract class IAccessTokenAnalyzerContract : IAccessTokenAnalyzer {
		private IAccessTokenAnalyzerContract() {
		}

		bool IAccessTokenAnalyzer.TryValidateAccessToken(IDirectedProtocolMessage message, string accessToken, out string user, out string scope) {
			Contract.Requires<ArgumentNullException>(message != null, "message");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(accessToken));
			Contract.Ensures(Contract.Result<bool>() == (Contract.ValueAtReturn<string>(out user) != null));
			
			throw new NotImplementedException();
		}
	}

}
