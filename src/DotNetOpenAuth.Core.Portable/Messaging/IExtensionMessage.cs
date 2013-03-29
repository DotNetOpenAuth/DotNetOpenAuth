//-----------------------------------------------------------------------
// <copyright file="IExtensionMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// An interface that extension messages must implement.
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "Extension messages may gain members later on.")]
	public interface IExtensionMessage : IMessage {
	}
}
