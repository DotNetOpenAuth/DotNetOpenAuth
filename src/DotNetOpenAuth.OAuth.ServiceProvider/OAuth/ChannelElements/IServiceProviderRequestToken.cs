//-----------------------------------------------------------------------
// <copyright file="IServiceProviderRequestToken.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// A description of a request token and its metadata as required by a Service Provider
	/// </summary>
	public interface IServiceProviderRequestToken {
		/// <summary>
		/// Gets the token itself.
		/// </summary>
		string Token { get; }

		/// <summary>
		/// Gets the consumer key that requested this token.
		/// </summary>
		string ConsumerKey { get; }

		/// <summary>
		/// Gets the (local) date that this request token was first created on.
		/// </summary>
		DateTime CreatedOn { get; }

		/// <summary>
		/// Gets or sets the callback associated specifically with this token, if any.
		/// </summary>
		/// <value>The callback URI; or <c>null</c> if no callback was specifically assigned to this token.</value>
		Uri Callback { get; set; }

		/// <summary>
		/// Gets or sets the verifier that the consumer must include in the <see cref="AuthorizedTokenRequest"/>
		/// message to exchange this request token for an access token.
		/// </summary>
		/// <value>The verifier code, or <c>null</c> if none has been assigned (yet).</value>
		string VerificationCode { get; set; }

		/// <summary>
		/// Gets or sets the version of the Consumer that requested this token.
		/// </summary>
		/// <remarks>
		/// This property is used to determine whether a <see cref="VerificationCode"/> must be
		/// generated when the user authorizes the Consumer or not.
		/// </remarks>
		Version ConsumerVersion { get; set; }
	}
}
