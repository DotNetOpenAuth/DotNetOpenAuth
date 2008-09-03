//-----------------------------------------------------------------------
// <copyright file="TestMessageTypeProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;

	internal class TestMessageTypeProvider : IMessageTypeProvider {
		#region IMessageTypeProvider Members

		public Type GetRequestMessageType(IDictionary<string, string> fields) {
			if (fields.ContainsKey("age")) {
				return typeof(TestMessage);
			} else {
				return null;
			}
		}

		public Type GetResponseMessageType(IProtocolMessage request, IDictionary<string, string> fields) {
			return this.GetRequestMessageType(fields);
		}

		#endregion
	}
}
