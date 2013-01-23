//-----------------------------------------------------------------------
// <copyright file="IMessageWithBinaryData.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;

	/// <summary>
	/// The interface that classes must implement to be serialized/deserialized
	/// as protocol or extension messages that uses POST multi-part data for binary content.
	/// </summary>
	public interface IMessageWithBinaryData : IDirectedProtocolMessage {
		/// <summary>
		/// Gets the parts of the message that carry binary data.
		/// </summary>
		/// <value>A list of parts.  Never null.</value>
		IList<MultipartContentMember> BinaryData { get; }

		/// <summary>
		/// Gets a value indicating whether this message should be sent as multi-part POST.
		/// </summary>
		bool SendAsMultipart { get; }
	}
}
