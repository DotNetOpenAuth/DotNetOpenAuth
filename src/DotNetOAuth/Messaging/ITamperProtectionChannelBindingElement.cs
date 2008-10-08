//-----------------------------------------------------------------------
// <copyright file="ITamperProtectionChannelBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	using System;
	using DotNetOAuth.ChannelElements;

	/// <summary>
	/// An interface that must be implemented by message transforms/validators in order
	/// to be included in the channel stack.
	/// </summary>
	public interface ITamperProtectionChannelBindingElement : IChannelBindingElement, ICloneable {
		/// <summary>
		/// Gets or sets the delegate that will initialize the non-serialized properties necessary on a signed
		/// message so that its signature can be correctly calculated for verification.
		/// May be null for Consumers (who never have to verify signatures).
		/// </summary>
		Action<ITamperResistantOAuthMessage> SignatureVerificationCallback { get; set; }
	}
}
