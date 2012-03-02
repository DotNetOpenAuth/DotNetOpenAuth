//-----------------------------------------------------------------------
// <copyright file="ITamperResistantProtocolMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	/// <summary>
	/// The contract a message that is signed must implement.
	/// </summary>
	/// <remarks>
	/// This type might have appeared in the DotNetOpenAuth.Messaging.Bindings namespace since
	/// it is only used by types in that namespace, but all those types are internal and this
	/// is the only one that was public.
	/// </remarks>
	public interface ITamperResistantProtocolMessage : IProtocolMessage {
		/// <summary>
		/// Gets or sets the message signature.
		/// </summary>
		string Signature { get; set; }
	}
}
