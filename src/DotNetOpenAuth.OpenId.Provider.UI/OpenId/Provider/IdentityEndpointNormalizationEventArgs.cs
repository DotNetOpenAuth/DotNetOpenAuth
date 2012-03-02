//-----------------------------------------------------------------------
// <copyright file="IdentityEndpointNormalizationEventArgs.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;

	/// <summary>
	/// The event arguments passed to the <see cref="IdentityEndpoint.NormalizeUri"/> event handler.
	/// </summary>
	public class IdentityEndpointNormalizationEventArgs : EventArgs {
		/// <summary>
		/// Initializes a new instance of the <see cref="IdentityEndpointNormalizationEventArgs"/> class.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The user supplied identifier.</param>
		internal IdentityEndpointNormalizationEventArgs(UriIdentifier userSuppliedIdentifier) {
			this.UserSuppliedIdentifier = userSuppliedIdentifier;
		}

		/// <summary>
		/// Gets or sets the portion of the incoming page request URI that is relevant to normalization.
		/// </summary>
		/// <remarks>
		/// This identifier should be used to look up the user whose identity page is being queried.
		/// It MAY be set in case some clever web server URL rewriting is taking place that ASP.NET
		/// does not know about but your site does. If this is the case this property should be set
		/// to whatever the original request URL was.
		/// </remarks>
		public Uri UserSuppliedIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the normalized form of the user's identifier, according to the host site's policy.
		/// </summary>
		/// <remarks>
		/// <para>This should be set to some constant value for an individual user.  
		/// For example, if <see cref="UserSuppliedIdentifier"/> indicates that identity page
		/// for "BOB" is being called up, then the following things should be considered:</para>
		/// <list>
		/// <item>Normalize the capitalization of the URL: for example, change http://provider/BOB to
		/// http://provider/bob.</item>
		/// <item>Switch to HTTPS is it is offered: change http://provider/bob to https://provider/bob.</item>
		/// <item>Strip off the query string if it is not part of the canonical identity:
		/// https://provider/bob?timeofday=now becomes https://provider/bob</item>
		/// <item>Ensure that any trailing slash is either present or absent consistently.  For example,
		/// change https://provider/bob/ to https://provider/bob.</item>
		/// </list>
		/// <para>When this property is set, the <see cref="IdentityEndpoint"/> control compares it to
		/// the request that actually came in, and redirects the browser to use the normalized identifier
		/// if necessary.</para>
		/// <para>Using the normalized identifier in the request is <i>very</i> important as it
		/// helps the user maintain a consistent identity across sites and across site visits to an individual site.
		/// For example, without normalizing the URL, Bob might sign into a relying party site as 
		/// http://provider/bob one day and https://provider/bob the next day, and the relying party
		/// site <i>should</i> interpret Bob as two different people because the URLs are different.
		/// By normalizing the URL at the Provider's identity page for Bob, whichever URL Bob types in
		/// from day-to-day gets redirected to a normalized form, so Bob is seen as the same person
		/// all the time, which is of course what Bob wants.
		/// </para>
		/// </remarks>
		public Uri NormalizedIdentifier { get; set; }
	}
}
