//-----------------------------------------------------------------------
// <copyright file="TestBaseMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	internal class TestBaseMessage : IProtocolMessage, IBaseMessageExplicitMembers {
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		[MessagePart("age", IsRequired = true)]
		public int Age { get; set; }

		[MessagePart]
		public string Name { get; set; }

		[MessagePart("explicit")]
		string IBaseMessageExplicitMembers.ExplicitProperty { get; set; }

		Version IMessage.Version {
			get { return new Version(1, 0); }
		}

		MessageProtections IProtocolMessage.RequiredProtection {
			get { return MessageProtections.None; }
		}

		MessageTransport IProtocolMessage.Transport {
			get { return MessageTransport.Indirect; }
		}

		IDictionary<string, string> IMessage.ExtraData {
			get { return this.extraData; }
		}

		internal string PrivatePropertyAccessor {
			get { return this.PrivateProperty; }
			set { this.PrivateProperty = value; }
		}

		[MessagePart("private")]
		private string PrivateProperty { get; set; }

		void IMessage.EnsureValidMessage() { }
	}
}
