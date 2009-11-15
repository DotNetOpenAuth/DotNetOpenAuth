//-----------------------------------------------------------------------
// <copyright file="Model.IssuedRequestToken.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth.ChannelElements;

	public partial class IssuedRequestToken : IServiceProviderRequestToken {
		/// <summary>
		/// Gets or sets the callback associated specifically with this token, if any.
		/// </summary>
		/// <value>
		/// The callback URI; or <c>null</c> if no callback was specifically assigned to this token.
		/// </value>
		public Uri Callback {
			get { return this.CallbackAsString != null ? new Uri(this.CallbackAsString) : null; }
			set { this.CallbackAsString = value != null ? value.AbsoluteUri : null; }
		}

		/// <summary>
		/// Gets or sets the version of the Consumer that requested this token.
		/// </summary>
		/// <remarks>
		/// This property is used to determine whether a <see cref="VerificationCode"/> must be
		/// generated when the user authorizes the Consumer or not.
		/// </remarks>
		Version IServiceProviderRequestToken.ConsumerVersion {
			get { return this.ConsumerVersionAsString != null ? new Version(this.ConsumerVersionAsString) : null; }
			set { this.ConsumerVersionAsString = value != null ? value.ToString() : null; }
		}

		/// <summary>
		/// Gets the consumer key that requested this token.
		/// </summary>
		string IServiceProviderRequestToken.ConsumerKey {
			get { return this.Consumer.ConsumerKey; }
		}

		/// <summary>
		/// Authorizes this request token to allow exchange for an access token.
		/// </summary>
		/// <remarks>
		/// Call this method when the user has completed web-based authorization.
		/// </remarks>
		public void Authorize() {
			this.User = Database.LoggedInUser;
			Database.DataContext.SaveChanges();
		}
	}
}
