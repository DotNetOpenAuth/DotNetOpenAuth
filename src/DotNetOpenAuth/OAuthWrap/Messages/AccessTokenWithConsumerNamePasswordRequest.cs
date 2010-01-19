//-----------------------------------------------------------------------
// <copyright file="AccessTokenWithConsumerNamePasswordRequest.cs" company="Andrew Arnott">
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
	/// A request for an access token for a consumer application that has its
	/// own (non-user affiliated) consumer name and password.
	/// </summary>
	internal class AccessTokenWithConsumerNamePasswordRequest : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenWithConsumerNamePasswordRequest"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		internal AccessTokenWithConsumerNamePasswordRequest(Version version)
			: base(version) {
		}

		/// <summary>
		/// Gets or sets the account name.
		/// </summary>
		/// <value>The consumer name.</value>
		[MessagePart(Protocol.sa_name, IsRequired = true, AllowEmpty = false)]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the account password.
		/// </summary>
		/// <value>The password.</value>
		[MessagePart(Protocol.sa_password, IsRequired = true, AllowEmpty = true)]
		public string Password { get; set; }
	}
}
