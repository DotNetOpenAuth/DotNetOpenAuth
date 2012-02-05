//-----------------------------------------------------------------------
// <copyright file="TestMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	internal abstract class TestMessage : IDirectResponseProtocolMessage {
		private MessageTransport transport;
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		protected TestMessage()
			: this(MessageTransport.Direct) {
		}

		protected TestMessage(MessageTransport transport) {
			this.transport = transport;
		}

		[MessagePart("age", IsRequired = true)]
		public int Age { get; set; }
		[MessagePart("Name")]
		public string Name { get; set; }
		[MessagePart]
		public string EmptyMember { get; set; }
		[MessagePart(null)] // null name tests that Location is still the name.
		public Uri Location { get; set; }
		[MessagePart(IsRequired = true)]
		public DateTime Timestamp { get; set; }

		#region IProtocolMessage Properties

		Version IMessage.Version {
			get { return new Version(1, 0); }
		}

		MessageProtections IProtocolMessage.RequiredProtection {
			get { return MessageProtections.None; }
		}

		MessageTransport IProtocolMessage.Transport {
			get { return this.transport; }
		}

		IDictionary<string, string> IMessage.ExtraData {
			get { return this.extraData; }
		}

		#endregion

		#region IDirectResponseProtocolMessage Members

		public IDirectedProtocolMessage OriginatingRequest { get; set; }

		#endregion

		#region IMessage Methods

		void IMessage.EnsureValidMessage() {
			if (this.EmptyMember != null || this.Age < 0) {
				throw new ProtocolException();
			}
		}

		#endregion
	}
}
