//-----------------------------------------------------------------------
// <copyright file="DataBag.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuthWrap.Messages;

	/// <summary>
	/// A collection of message parts that will be serialized into a single string,
	/// to be set into a larger message.
	/// </summary>
	internal abstract class DataBag : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="DataBag"/> class.
		/// </summary>
		protected DataBag()
			: base(Protocol.Default.Version) {
		}

		/// <summary>
		/// Gets or sets the nonce.
		/// </summary>
		/// <value>The nonce.</value>
		[MessagePart]
		internal byte[] Nonce { get; set; }

		/// <summary>
		/// Gets or sets the UTC creation date of this token.
		/// </summary>
		/// <value>The UTC creation date.</value>
		[MessagePart("timestamp", IsRequired = true, Encoder = typeof(TimestampEncoder))]
		internal DateTime UtcCreationDate { get; set; }

		/// <summary>
		/// Gets or sets the signature.
		/// </summary>
		/// <value>The signature.</value>
		[MessagePart("sig")]
		internal byte[] Signature { get; set; }

		/// <summary>
		/// Gets or sets the message that delivered this DataBag instance to this host.
		/// </summary>
		protected internal IProtocolMessage ContainingMessage { get; set; }

		/// <summary>
		/// Gets the type of this instance.
		/// </summary>
		/// <value>The type of the bag.</value>
		/// <remarks>
		/// This ensures that one token cannot be misused as another kind of token.
		/// </remarks>
		[MessagePart("t", IsRequired = true, AllowEmpty = false)]
		private string BagType {
			get { return this.GetType().Name; }
		}
	}
}
