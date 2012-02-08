//-----------------------------------------------------------------------
// <copyright file="OAuth1Principal.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;

	/// <summary>
	/// Represents an OAuth consumer that is impersonating a known user on the system.
	/// </summary>
	[SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable", Justification = "Not cocreatable.")]
	[Serializable]
	[ComVisible(true)]
	internal class OAuth1Principal : OAuthPrincipal {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth1Principal"/> class.
		/// </summary>
		/// <param name="token">The access token.</param>
		internal OAuth1Principal(IServiceProviderAccessToken token)
			: base(token.Username, token.Roles) {
			Requires.NotNull(token, "token");

			this.AccessToken = token.Token;
		}
	}
}
