//-----------------------------------------------------------------------
// <copyright file="MockTransformationBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	internal class MockTransformationBindingElement : IChannelBindingElement {
		private string transform;

		internal MockTransformationBindingElement(string transform) {
			if (transform == null) {
				throw new ArgumentNullException("transform");
			}

			this.transform = transform;
		}

		#region IChannelBindingElement Members

		MessageProtection IChannelBindingElement.Protection {
			get { return MessageProtection.None; }
		}

		bool IChannelBindingElement.PrepareMessageForSending(IProtocolMessage message) {
			var testMessage = message as TestMessage;
			if (testMessage != null) {
				testMessage.Name = this.transform + testMessage.Name;
				return true;
			}

			return false;
		}

		bool IChannelBindingElement.PrepareMessageForReceiving(IProtocolMessage message) {
			var testMessage = message as TestMessage;
			if (testMessage != null) {
				StringAssert.StartsWith(testMessage.Name, this.transform);
				testMessage.Name = testMessage.Name.Substring(this.transform.Length);
				return true;
			}

			return false;
		}

		#endregion
	}
}
