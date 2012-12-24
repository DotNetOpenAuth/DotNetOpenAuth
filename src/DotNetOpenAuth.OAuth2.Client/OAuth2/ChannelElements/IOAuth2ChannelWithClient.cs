//-----------------------------------------------------------------------
// <copyright file="IOAuth2ChannelWithClient.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml;

	/// <summary>
	/// An interface that defines the OAuth2 client specific channel additions.
	/// </summary>
	internal interface IOAuth2ChannelWithClient {
		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the client credentials applicator extension to use.
		/// </summary>
		ClientCredentialApplicator ClientCredentialApplicator { get; set; }

		/// <summary>
		/// Gets quotas used when deserializing JSON.
		/// </summary>
		XmlDictionaryReaderQuotas JsonReaderQuotas { get; }
	}
}
