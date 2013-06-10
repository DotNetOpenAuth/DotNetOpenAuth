//-----------------------------------------------------------------------
// <copyright file="ITamperProtectionChannelBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// An interface that must be implemented by message transforms/validators in order
	/// to be included in the channel stack.
	/// </summary>
	public interface ITamperProtectionChannelBindingElement : IChannelBindingElement {
		/// <summary>
		/// Gets or sets the delegate that will initialize the non-serialized properties necessary on a
		/// signable message so that its signature can be correctly calculated or verified.
		/// </summary>
		Action<ITamperResistantOAuthMessage> SignatureCallback { get; set; }

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>The cloned instance.</returns>
		ITamperProtectionChannelBindingElement Clone();
	}
}
