//-----------------------------------------------------------------------
// <copyright file="ProtocolException.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Security.Permissions;

	/// <summary>
	/// An exception to represent errors in the local or remote implementation of the protocol.
	/// </summary>
	[Serializable]
	public class ProtocolException : Exception, IDirectedProtocolMessage {
		/// <summary>
		/// The request message being processed when this exception was generated, if any.
		/// </summary>
		private IProtocolMessage inResponseTo;

		/// <summary>
		/// The indirect message receiver this exception should be delivered to, if any.
		/// </summary>
		private Uri recipient;

		/// <summary>
		/// A cache for extra name/value pairs tacked on as data when this exception is sent as a message.
		/// </summary>
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolException"/> class.
		/// </summary>
		public ProtocolException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolException"/> class.
		/// </summary>
		/// <param name="message">A message describing the specific error the occurred or was detected.</param>
		public ProtocolException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolException"/> class.
		/// </summary>
		/// <param name="message">A message describing the specific error the occurred or was detected.</param>
		/// <param name="inner">The inner exception to include.</param>
		public ProtocolException(string message, Exception inner) : base(message, inner) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolException"/> class
		/// such that it can be sent as a protocol message response to a remote caller.
		/// </summary>
		/// <param name="message">The human-readable exception message.</param>
		/// <param name="faultedMessage">The message that was the cause of the exception.  May not be null.</param>
		internal ProtocolException(string message, IProtocolMessage faultedMessage)
			: base(message) {
			if (faultedMessage == null) {
				throw new ArgumentNullException("faultedMessage");
			}

			this.FaultedMessage = faultedMessage;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolException"/> class
		/// such that it can be sent as a protocol message response to a remote caller.
		/// </summary>
		/// <param name="message">The human-readable exception message.</param>
		/// <param name="inResponseTo">
		/// If <paramref name="message"/> is a response to an incoming message, this is the incoming message.
		/// This is useful for error scenarios in deciding just how to send the response message.
		/// May be null.
		/// </param>
		/// <param name="remoteIndirectReceiver">
		/// In the case of exceptions that will be sent as indirect messages to the original calling
		/// remote party, this is the URI of that remote site's receiver.
		/// May be null only if the <paramref name="inResponseTo"/> message is a direct request.
		/// </param>
		internal ProtocolException(string message, IProtocolMessage inResponseTo, Uri remoteIndirectReceiver)
			: this(message) {
			if (inResponseTo == null) {
				throw new ArgumentNullException("inResponseTo");
			}
			this.inResponseTo = inResponseTo;
			this.FaultedMessage = inResponseTo;

			if (remoteIndirectReceiver == null && inResponseTo.Transport != MessageTransport.Direct) {
				// throw an exception, with ourselves as the inner exception (as fully initialized as we can be).
				throw new ArgumentNullException("remoteIndirectReceiver");
			}
			this.recipient = remoteIndirectReceiver;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtocolException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> 
		/// that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext 
		/// that contains contextual information about the source or destination.</param>
		protected ProtocolException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) {
			throw new NotImplementedException();
		}

		#region IProtocolMessage Properties

		/// <summary>
		/// Gets the version of the protocol this message is prepared to implement.
		/// </summary>
		Version IProtocolMessage.ProtocolVersion {
			get { return this.ProtocolVersion; }
		}

		/// <summary>
		/// Gets the level of protection this exception requires when transmitted as a message.
		/// </summary>
		MessageProtections IProtocolMessage.RequiredProtection {
			get { return RequiredProtection; }
		}

		/// <summary>
		/// Gets whether this is a direct or indirect message.
		/// </summary>
		MessageTransport IProtocolMessage.Transport {
			get { return this.Transport; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this message was deserialized as an incoming message.
		/// </summary>
		bool IProtocolMessage.Incoming { get; set; }

		#endregion

		#region IDirectedProtocolMessage Members

		/// <summary>
		/// Gets the preferred method of transport for the message.
		/// </summary>
		/// <remarks>
		/// This exception may be delivered as an indirect message or a direct response.  This property
		/// only applies to the former.  The latter will never query this property.
		/// </remarks>
		HttpDeliveryMethods IDirectedProtocolMessage.HttpMethods {
			get { return HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest; }
		}

		/// <summary>
		/// Gets the URL of the intended receiver of this message.
		/// </summary>
		/// <remarks>
		/// This property should only be called when the error is being sent as an indirect response.
		/// </remarks>
		Uri IDirectedProtocolMessage.Recipient {
			get { return this.Recipient; }
		}

		#endregion

		/// <summary>
		/// Gets the dictionary of additional name/value fields tacked on to this message.
		/// </summary>
		IDictionary<string, string> IProtocolMessage.ExtraData {
			get { return this.ExtraData; }
		}

		/// <summary>
		/// Gets the message that caused the exception.
		/// </summary>
		internal IProtocolMessage FaultedMessage {
			get;
			private set;
		}

		/// <summary>
		/// Gets the level of protection this exception requires when transmitted as a message.
		/// </summary>
		protected static MessageProtections RequiredProtection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Gets the URL of the intended receiver of this message.
		/// </summary>
		/// <remarks>
		/// This property should only be called when the error is being sent as an indirect response.
		/// </remarks>
		protected Uri Recipient {
			get {
				if (this.inResponseTo == null) {
					throw new InvalidOperationException(MessagingStrings.ExceptionNotConstructedForTransit);
				}
				return this.recipient;
			}
		}

		/// <summary>
		/// Gets the version of the protocol this message is prepared to implement.
		/// </summary>
		protected Version ProtocolVersion {
			get {
				if (this.inResponseTo == null) {
					throw new InvalidOperationException(MessagingStrings.ExceptionNotConstructedForTransit);
				}
				return this.inResponseTo.ProtocolVersion;
			}
		}

		/// <summary>
		/// Gets whether this is a direct or indirect message.
		/// </summary>
		protected MessageTransport Transport {
			get {
				if (this.inResponseTo == null) {
					throw new InvalidOperationException(MessagingStrings.ExceptionNotConstructedForTransit);
				}
				return this.inResponseTo.Transport;
			}
		}

		/// <summary>
		/// Gets the dictionary of additional name/value fields tacked on to this message.
		/// </summary>
		protected IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

		/// <summary>
		/// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic).
		/// </exception>
		/// <PermissionSet>
		/// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/>
		/// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/>
		/// </PermissionSet>
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) {
			base.GetObjectData(info, context);
			throw new NotImplementedException();
		}

		#region IProtocolMessage Methods

		/// <summary>
		/// See <see cref="IProtocolMessage.EnsureValidMessage"/>.
		/// </summary>
		void IProtocolMessage.EnsureValidMessage() {
			this.EnsureValidMessage();
		}

		/// <summary>
		/// See <see cref="IProtocolMessage.EnsureValidMessage"/>.
		/// </summary>
		protected virtual void EnsureValidMessage() {
			if (this.inResponseTo == null) {
				throw new InvalidOperationException(MessagingStrings.ExceptionNotConstructedForTransit);
			}
		}

		#endregion
	}
}
