namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal interface IOAuth2ChannelWithClient {
		/// <summary>
		/// Gets or sets the identifier by which this client is known to the Authorization Server.
		/// </summary>
		string ClientIdentifier { get; set; }

		ClientCredentialApplicator ClientCredentialApplicator { get; set; }
	}
}
