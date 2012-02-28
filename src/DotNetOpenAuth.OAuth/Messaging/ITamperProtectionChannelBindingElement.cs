//-----------------------------------------------------------------------
// <copyright file="ITamperProtectionChannelBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// An interface that must be implemented by message transforms/validators in order
	/// to be included in the channel stack.
	/// </summary>
	[ContractClass(typeof(ITamperProtectionChannelBindingElementContract))]
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

	/// <summary>
	/// Contract class for the <see cref="ITamperProtectionChannelBindingElement"/> interface.
	/// </summary>
	[ContractClassFor(typeof(ITamperProtectionChannelBindingElement))]
	internal abstract class ITamperProtectionChannelBindingElementContract : ITamperProtectionChannelBindingElement {
		#region ITamperProtectionChannelBindingElement Properties

		/// <summary>
		/// Gets or sets the delegate that will initialize the non-serialized properties necessary on a
		/// signable message so that its signature can be correctly calculated or verified.
		/// </summary>
		Action<ITamperResistantOAuthMessage> ITamperProtectionChannelBindingElement.SignatureCallback {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		#endregion

		#region IChannelBindingElement Members

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		/// <remarks>
		/// This property is set by the channel when it is first constructed.
		/// </remarks>
		Channel IChannelBindingElement.Channel {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Gets the protection commonly offered (if any) by this binding element.
		/// </summary>
		/// <remarks>
		/// This value is used to assist in sorting binding elements in the channel stack.
		/// </remarks>
		MessageProtections IChannelBindingElement.Protection {
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		MessageProtections? IChannelBindingElement.ProcessOutgoingMessage(IProtocolMessage message) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		MessageProtections? IChannelBindingElement.ProcessIncomingMessage(IProtocolMessage message) {
			throw new NotImplementedException();
		}

		#endregion

		#region ITamperProtectionChannelBindingElement Methods

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>The cloned instance.</returns>
		ITamperProtectionChannelBindingElement ITamperProtectionChannelBindingElement.Clone() {
			Contract.Ensures(Contract.Result<ITamperProtectionChannelBindingElement>() != null);
			throw new NotImplementedException();
		}

		#endregion
	}
}
