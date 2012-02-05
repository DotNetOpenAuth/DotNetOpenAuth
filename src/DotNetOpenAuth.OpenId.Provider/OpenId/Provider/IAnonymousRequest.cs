//-----------------------------------------------------------------------
// <copyright file="IAnonymousRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Instances of this interface represent incoming extension-only requests.
	/// This interface provides the details of the request and allows setting
	/// the response.
	/// </summary>
	public interface IAnonymousRequest : IHostProcessedRequest {
		/// <summary>
		/// Gets or sets a value indicating whether the user approved sending any data to the relying party.
		/// </summary>
		/// <value><c>true</c> if approved; otherwise, <c>false</c>.</value>
		bool? IsApproved { get; set; }
	}
}
