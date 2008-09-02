using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetOAuth.Messaging {
	internal interface IMessageTypeProvider {
		/// <summary>
		/// Analyzes a message payload to discover what kind of message is embedded in it.
		/// </summary>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <returns>The <see cref="IProtocolMessage"/>-derived concrete class that
		/// this message can deserialize to.</returns>
		Type GetMessageType(IDictionary<string, string> fields);
	}
}
