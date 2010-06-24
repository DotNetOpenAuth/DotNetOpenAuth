//-----------------------------------------------------------------------
// <copyright file="AuthorizationDataBag.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// A data bag that stores authorization data.
	/// </summary>
	internal abstract class AuthorizationDataBag : DataBag, IAuthorizationDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationDataBag"/> class.
		/// </summary>
		protected AuthorizationDataBag() {
		}

		/// <summary>
		/// Gets or sets the identifier of the client authorized to access protected data.
		/// </summary>
		/// <value></value>
		[MessagePart]
		public string ClientIdentifier { get; set; }

		/// <summary>
		/// Gets the date this authorization was established or the token was issued.
		/// </summary>
		/// <value>A date/time expressed in UTC.</value>
		public DateTime UtcIssued {
			get { return this.UtcCreationDate; }
		}

		/// <summary>
		/// Gets or sets the name on the account whose data on the resource server is accessible using this authorization.
		/// </summary>
		[MessagePart]
		public string User { get; set; }

		/// <summary>
		/// Gets or sets the scope of operations the client is allowed to invoke.
		/// </summary>
		[MessagePart]
		public string Scope { get; set; }
	}
}
