//-----------------------------------------------------------------------
// <copyright file="AccessProtectedResourceRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Net.Http;

	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message attached to a request for protected resources that provides the necessary
	/// credentials to be granted access to those resources.
	/// </summary>
	public class AccessProtectedResourceRequest : SignedMessageBase, ITokenContainingMessage, IMessageWithBinaryData {
		/// <summary>
		/// A store for the binary data that is carried in the message.
		/// </summary>
		private List<MultipartContentMember> binaryData = new List<MultipartContentMember>();

		/// <summary>
		/// Initializes a new instance of the <see cref="AccessProtectedResourceRequest"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		/// <param name="version">The OAuth version.</param>
		protected internal AccessProtectedResourceRequest(MessageReceivingEndpoint serviceProvider, Version version)
			: base(MessageTransport.Direct, serviceProvider, version) {
		}

		/// <summary>
		/// Gets or sets the Token.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This property IS accessible by a different name.")]
		string ITokenContainingMessage.Token {
			get { return this.AccessToken; }
			set { this.AccessToken = value; }
		}

		/// <summary>
		/// Gets or sets the Access Token.
		/// </summary>
		/// <remarks>
		/// In addition to just allowing OAuth to verify a valid message,
		/// this property is useful on the Service Provider to verify that the access token
		/// has proper authorization for the resource being requested, and to know the
		/// context around which user provided the authorization.
		/// </remarks>
		[MessagePart("oauth_token", IsRequired = true)]
		public string AccessToken { get; set; }

		#region IMessageWithBinaryData Members

		/// <summary>
		/// Gets the parts of the message that carry binary data.
		/// </summary>
		/// <value>A list of parts.  Never null.</value>
		public IList<MultipartContentMember> BinaryData {
			get { return this.binaryData; }
		}

		/// <summary>
		/// Gets a value indicating whether this message should be sent as multi-part POST.
		/// </summary>
		public bool SendAsMultipart {
			get { return this.HttpMethod == HttpMethod.Post && this.BinaryData.Count > 0; }
		}

		#endregion
	}
}
