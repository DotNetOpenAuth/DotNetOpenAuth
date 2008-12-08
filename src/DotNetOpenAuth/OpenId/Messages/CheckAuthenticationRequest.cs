//-----------------------------------------------------------------------
// <copyright file="CheckAuthenticationRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A message a Relying Party sends to a Provider to confirm the validity
	/// of a positive assertion that was signed by a Provider-only secret.
	/// </summary>
	/// <remarks>
	/// The significant payload of this message depends entirely upon the
	/// assertion message, and therefore is all in the 
	/// <see cref="DotNetOpenAuth.Messaging.IProtocolMessage.ExtraData"/> property bag.
	/// </remarks>
	internal class CheckAuthenticationRequest : RequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckAuthenticationRequest"/> class.
		/// </summary>
		/// <param name="version">The OpenID version this message must comply with.</param>
		/// <param name="providerEndpoint">The OpenID Provider endpoint.</param>
		internal CheckAuthenticationRequest(Version version, Uri providerEndpoint)
			: base(version, providerEndpoint, GetProtocolConstant(version, p => p.Args.Mode.check_authentication), DotNetOpenAuth.Messaging.MessageTransport.Direct) {
		}
	}
}
