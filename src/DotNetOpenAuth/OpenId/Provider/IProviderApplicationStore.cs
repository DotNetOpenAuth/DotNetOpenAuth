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
	/// A hybrid of all the store interfaces that a Provider requires in order
	/// to operate in "smart" mode.
	/// </summary>
	public interface IProviderApplicationStore : IAssociationStore<AssociationRelyingPartyType>, INonceStore {
	}
}
