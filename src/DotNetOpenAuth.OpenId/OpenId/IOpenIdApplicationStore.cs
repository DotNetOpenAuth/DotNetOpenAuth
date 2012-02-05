//-----------------------------------------------------------------------
// <copyright file="IOpenIdApplicationStore.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// A hybrid of the store interfaces that an OpenID Provider must implement, and
	/// an OpenID Relying Party may implement to operate in stateful (smart) mode.
	/// </summary>
	public interface IOpenIdApplicationStore : ICryptoKeyStore, INonceStore {
	}
}
