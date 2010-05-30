//-----------------------------------------------------------------------
// <copyright file="IAuthorizationServer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using DotNetOpenAuth.Messaging.Bindings;

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth.ChannelElements;

	[ContractClass(typeof(IAuthorizationServerContract))]
	public interface IAuthorizationServer {
		IConsumerDescription GetClient(string clientIdentifier);

		byte[] Secret { get; }

		INonceStore VerificationCodeNonceStore { get; }
	}

	[ContractClassFor(typeof(IAuthorizationServer))]
	internal abstract class IAuthorizationServerContract : IAuthorizationServer {
		private IAuthorizationServerContract() {
		}

		IConsumerDescription IAuthorizationServer.GetClient(string clientIdentifier) {
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(clientIdentifier));
			Contract.Ensures(Contract.Result<IConsumerDescription>() != null);
			throw new NotImplementedException();
		}

		byte[] IAuthorizationServer.Secret {
			get {
				Contract.Ensures(Contract.Result<byte[]>() != null);
				throw new NotImplementedException();
			}
		}

		INonceStore IAuthorizationServer.VerificationCodeNonceStore {
			get {
				Contract.Ensures(Contract.Result<INonceStore>() != null);
				throw new NotImplementedException();
			}
		}
	}

}
