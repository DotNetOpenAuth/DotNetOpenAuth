//-----------------------------------------------------------------------
// <copyright file="AuthServerBindingElementBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Messaging;

	/// <summary>
	/// The base class for any authorization server channel binding element.
	/// </summary>
	internal abstract class AuthServerBindingElementBase : IChannelBindingElement {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthServerBindingElementBase"/> class.
		/// </summary>
		protected AuthServerBindingElementBase() {
		}

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		/// <remarks>
		/// This property is set by the channel when it is first constructed.
		/// </remarks>
		public Channel Channel { get; set; }

		/// <summary>
		/// Gets the protection commonly offered (if any) by this binding element.
		/// </summary>
		/// <remarks>
		/// This value is used to assist in sorting binding elements in the channel stack.
		/// </remarks>
		public abstract MessageProtections Protection { get; }

		/// <summary>
		/// Gets the channel to which this binding element belongs.
		/// </summary>
		internal IOAuth2ChannelWithAuthorizationServer AuthServerChannel {
			get { return (IOAuth2ChannelWithAuthorizationServer)this.Channel; }
		}

		/// <summary>
		/// Gets the authorization server hosting this channel.
		/// </summary>
		/// <value>The authorization server.</value>
		protected IAuthorizationServerHost AuthorizationServer {
			get { return ((IOAuth2ChannelWithAuthorizationServer)this.Channel).AuthorizationServer; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection" /> properties where applicable.
		/// </remarks>
		public abstract Task<MessageProtections?> ProcessOutgoingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken);

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		public abstract Task<MessageProtections?> ProcessIncomingMessageAsync(IProtocolMessage message, CancellationToken cancellationToken);
	}
}
