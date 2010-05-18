//-----------------------------------------------------------------------
// <copyright file="IAuthorizationServer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.Contracts;

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth.ChannelElements;

	[ContractClass(typeof(IAuthorizationServerContract))]
	public interface IAuthorizationServer {
		IConsumerDescription GetClient(string clientIdentifier);
	}

	[ContractClassFor(typeof(IAuthorizationServer))]
	internal abstract class IAuthorizationServerContract : IAuthorizationServer {
		private IAuthorizationServerContract() {
		}

		IConsumerDescription IAuthorizationServer.GetClient(string clientIdentifier) {
			Contract.Ensures(Contract.Result<IConsumerDescription>() != null);
			throw new NotImplementedException();
		}
	}

}
