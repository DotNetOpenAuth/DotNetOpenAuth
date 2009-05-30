using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.OpenId.Messages;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.Messaging;

public class BadRequest : RequestBase {
	/// <summary>
	/// Initializes a new instance of the <see cref="BadRequest"/> class
	/// as a direct message.
	/// </summary>
	/// <param name="providerEndpoint">The provider endpoint.</param>
	public BadRequest(Uri providerEndpoint)
		: base(Protocol.V20.Version, providerEndpoint, "badmode", MessageTransport.Direct) {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BadRequest"/> class
	/// as an indirect message.
	/// </summary>
	/// <param name="providerEndpoint">The provider endpoint.</param>
	/// <param name="returnTo">The return to.</param>
	public BadRequest(Uri providerEndpoint, Uri returnTo)
		: base(Protocol.V20.Version, providerEndpoint, "badmode", MessageTransport.Indirect) {
		this.ReturnTo = returnTo;
	}

	[MessagePart("openid.return_to", IsRequired = false, AllowEmpty = false)]
	public Uri ReturnTo { get; set; }
}
