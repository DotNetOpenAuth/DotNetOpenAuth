//-----------------------------------------------------------------------
// <copyright file="InteropHelper.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId;

	public static class InteropHelper {
		public static void SpreadExtensions(this RelyingParty.IAuthenticationRequest request) {
			var req = (RelyingParty.AuthenticationRequest)request;
			throw new NotImplementedException();
		}

		public static void UnifyExtensions(this RelyingParty.IAuthenticationResponse response) {
			var resp = (RelyingParty.IAuthenticationResponse)response;
			throw new NotImplementedException();
		}

		public static void UnifyExtensions(this Provider.IAuthenticationRequest request) {
			var req = (Provider.AuthenticationRequest)request;
			throw new NotImplementedException();
		}
	}
}
