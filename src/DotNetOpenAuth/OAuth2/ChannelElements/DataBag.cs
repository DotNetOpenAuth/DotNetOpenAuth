//-----------------------------------------------------------------------
// <copyright file="DataBag.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;

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
