//-----------------------------------------------------------------------
// <copyright file="IErrorMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Members found on error response messages sent from a Provider 
	/// to a Relying Party in response to direct and indirect message
	/// requests that result in an error.
	/// </summary>
	internal interface IErrorMessage : IProtocolMessage {
		/// <summary>
		/// Gets or sets a human-readable message indicating why the request failed. 
		/// </summary>
		string ErrorMessage { get; set; }

		/// <summary>
		/// Gets or sets the contact address for the administrator of the server. 
		/// </summary>
		/// <value>The contact address may take any form, as it is intended to be displayed to a person. </value>
		string Contact { get; set; }

		/// <summary>
		/// Gets or sets a reference token, such as a support ticket number or a URL to a news blog, etc. 
		/// </summary>
		string Reference { get; set; }
	}
}
