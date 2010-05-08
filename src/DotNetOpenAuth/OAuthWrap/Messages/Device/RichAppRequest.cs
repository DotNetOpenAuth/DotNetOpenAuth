//-----------------------------------------------------------------------
// <copyright file="RichAppRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A request from a rich app Client to an Authorization Server requested 
	/// authorization to access user Protected Data.
	/// </summary>
	internal class RichAppRequest : MessageBase {
		[MessagePart(Protocol.type, IsRequired = true)]
		private const string MessageType = "device_code";

		/// <summary>
		/// Initializes a new instance of the <see cref="RichAppRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The authorization server.</param>
		/// <param name="version">The version.</param>
		internal RichAppRequest(Uri tokenEndpoint, Version version)
			: base(version, MessageTransport.Direct, tokenEndpoint) {
			this.HttpMethods = HttpDeliveryMethods.GetRequest;
		}

		/// <summary>
		/// Gets or sets the client identifier previously obtained from the Authorization Server.
		/// </summary>
		/// <value>The client identifier.</value>
		[MessagePart(Protocol.client_id, IsRequired = true, AllowEmpty = false)]
		internal string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the scope.
		/// </summary>
		/// <value>The Authorization Server MAY define authorization scope values for the Client to include.</value>
		[MessagePart(Protocol.scope, IsRequired = false, AllowEmpty = true)]
		internal string Scope { get; set; }
	}
}
