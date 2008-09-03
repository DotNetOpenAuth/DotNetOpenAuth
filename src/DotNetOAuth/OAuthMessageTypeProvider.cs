//-----------------------------------------------------------------------
// <copyright file="OAuthMessageTypeProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;

	internal class OAuthMessageTypeProvider : IMessageTypeProvider {
		#region IMessageTypeProvider Members

		public Type GetRequestMessageType(IDictionary<string, string> fields) {
			throw new NotImplementedException();
		}

		public Type GetResponseMessageType(IProtocolMessage request, IDictionary<string, string> fields) {
			throw new NotImplementedException();
		}

		#endregion
	}
}
