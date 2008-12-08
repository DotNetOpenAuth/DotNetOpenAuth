//-----------------------------------------------------------------------
// <copyright file="CheckAuthenticationResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The message sent from the Provider to the Relying Party to confirm/deny
	/// the validity of an assertion that was signed by a private Provider secret.
	/// </summary>
	internal class CheckAuthenticationResponse : DirectResponseBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckAuthenticationResponse"/> class.
		/// </summary>
		/// <param name="request">The request that this message is responding to.</param>
		internal CheckAuthenticationResponse(CheckAuthenticationRequest request)
			: base(request) {
		}

		/// <summary>
		/// Gets or sets a value indicating whether the signature of the verification request is valid.
		/// </summary>
		[MessagePart("is_valid", IsRequired = true)]
		internal bool IsValid { get; set; }

		/// <summary>
		/// Gets or sets the handle the relying party should invalidate if <see cref="IsValid"/> is true.
		/// </summary>
		/// <value>The "invalidate_handle" value sent in the verification request, if the OP confirms it is invalid.</value>
		/// <remarks>
		/// <para>If present in a verification response with "is_valid" set to "true",
		/// the Relying Party SHOULD remove the corresponding association from 
		/// its store and SHOULD NOT send further authentication requests with 
		/// this handle.</para>
		/// <para>This two-step process for invalidating associations is necessary 
		/// to prevent an attacker from invalidating an association at will by 
		/// adding "invalidate_handle" parameters to an authentication response.</para>
		/// </remarks>
		[MessagePart("invalidate_handle", IsRequired = false, AllowEmpty = false)]
		internal string InvalidateHandle { get; set; }
	}
}
