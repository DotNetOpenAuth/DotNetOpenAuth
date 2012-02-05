//-----------------------------------------------------------------------
// <copyright file="MockTransformationBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using NUnit.Framework;

	internal class MockTransformationBindingElement : IChannelBindingElement {
		private string transform;

		internal MockTransformationBindingElement(string transform) {
			if (transform == null) {
				throw new ArgumentNullException("transform");
			}

			this.transform = transform;
		}

		#region IChannelBindingElement Members

		MessageProtections IChannelBindingElement.Protection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		public Channel Channel { get; set; }

		MessageProtections? IChannelBindingElement.ProcessOutgoingMessage(IProtocolMessage message) {
			var testMessage = message as TestMessage;
			if (testMessage != null) {
				testMessage.Name = this.transform + testMessage.Name;
				return MessageProtections.None;
			}

			return null;
		}

		MessageProtections? IChannelBindingElement.ProcessIncomingMessage(IProtocolMessage message) {
			var testMessage = message as TestMessage;
			if (testMessage != null) {
				StringAssert.StartsWith(this.transform, testMessage.Name);
				testMessage.Name = testMessage.Name.Substring(this.transform.Length);
				return MessageProtections.None;
			}

			return null;
		}

		#endregion
	}
}
