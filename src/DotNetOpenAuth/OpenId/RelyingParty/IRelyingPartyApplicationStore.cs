//-----------------------------------------------------------------------
// <copyright file="IRelyingPartyApplicationStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.ChannelElements;

	/// <summary>
	/// A hybrid of all the store interfaces that a Relying Party requires in order
	/// to operate in "smart" mode.
	/// </summary>
	public interface IRelyingPartyApplicationStore : IAssociationStore<Uri>, INonceStore {
	}
}
