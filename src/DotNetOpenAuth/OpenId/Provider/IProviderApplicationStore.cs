//-----------------------------------------------------------------------
// <copyright file="IProviderApplicationStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// A hybrid of all the store interfaces that an OpenID Provider must implement.
	/// </summary>
	public interface IProviderApplicationStore : ICryptoKeyStore, INonceStore {
	}
}
