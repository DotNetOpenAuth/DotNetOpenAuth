//-----------------------------------------------------------------------
// <copyright file="DeviceResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An indirect response from the Authorization Server to the rich app Client
	/// with the verification code.
	/// </summary>
	internal class DeviceResponse : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal DeviceResponse(DeviceRequest request)
			: base(request) {
		}

		/// <summary>
		/// Gets or sets the verification code.
		/// </summary>
		/// <value>
		/// The long-lived credential assigned by the Authorization Server to this Client for
		/// use in accessing the authorizing user's protected resources.
		/// </value>
		[MessagePart(Protocol.code, IsRequired = true, AllowEmpty = false)]
		internal string VerificationCode { get; set; }

		/// <summary>
		/// Gets or sets the code the user must enter on the authorization page.
		/// </summary>
		/// <value>The user code.</value>
		[MessagePart(Protocol.user_code, IsRequired = true, AllowEmpty = false)]
		internal string UserCode { get; set; }

		/// <summary>
		/// Gets or sets the user authorization URI on the authorization server. 
		/// </summary>
		/// <value>
		/// REQUIRED. The end-user verification URI on the authorization server. The URI should be short and easy to remember as end-users will be asked to manually type it into their user-agent.
		/// </value>
		[MessagePart(Protocol.verification_uri, IsRequired = true)]
		internal Uri VerificationUri { get; set; }

		/// <summary>
		/// Gets or sets the lifetime.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Protocol.expires_in, IsRequired = false, Encoder = typeof(TimespanSecondsEncoder))]
		internal TimeSpan? Lifetime { get; set; }

		/// <summary>
		/// Gets or sets the minimum amount of time that the client SHOULD wait between polling requests to the token endpoint. 
		/// </summary>
		[MessagePart(Protocol.interval, IsRequired = false, Encoder = typeof(TimespanSecondsEncoder))]
		internal TimeSpan? PollingInterval { get; set; }

		/// <summary>
		/// Gets a value indicating whether the user granted the authorization request.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if authorization is granted; otherwise, <c>false</c>.
		/// </value>
		internal bool IsGranted {
			get { return !string.IsNullOrEmpty(this.VerificationCode) && this.VerificationCode != Protocol.user_denied; }
		}
	}
}
