//-----------------------------------------------------------------------
// <copyright file="AuthorizationDataBag.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;

	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A data bag that stores authorization data.
	/// </summary>
	public abstract class AuthorizationDataBag : DataBag, IAuthorizationDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationDataBag"/> class.
		/// </summary>
		protected AuthorizationDataBag() {
			this.Scope = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
		}

		/// <summary>
		/// Gets or sets the identifier of the client authorized to access protected data.
		/// </summary>
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
		/// Gets the scope of operations the client is allowed to invoke.
		/// </summary>
		[MessagePart(Encoder = typeof(ScopeEncoder))]
		public HashSet<string> Scope { get; private set; }
	}
}
