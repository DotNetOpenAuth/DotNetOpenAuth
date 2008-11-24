//-----------------------------------------------------------------------
// <copyright file="DiscoveryResult.cs" company="Scott Hanselman, Andrew Arnott">
//     Copyright (c) Scott Hanselman, Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Yadis {
	using System;
	using System.IO;
	using System.Net.Mime;
	using System.Web.UI.HtmlControls;
	using System.Xml;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Contains the result of YADIS discovery.
	/// </summary>
	internal class DiscoveryResult {
		/// <summary>
		/// Initializes a new instance of the <see cref="DiscoveryResult"/> class.
		/// </summary>
		/// <param name="requestUri">The user-supplied identifier.</param>
		/// <param name="initialResponse">The initial response.</param>
		/// <param name="finalResponse">The final response.</param>
		public DiscoveryResult(Uri requestUri, DirectWebResponse initialResponse, DirectWebResponse finalResponse) {
			RequestUri = requestUri;
			NormalizedUri = initialResponse.FinalUri;
			if (finalResponse == null) {
				ContentType = initialResponse.ContentType;
				ResponseText = initialResponse.Body;
				IsXrds = ContentType.MediaType == ContentTypes.Xrds;
			} else {
				ContentType = finalResponse.ContentType;
				ResponseText = finalResponse.Body;
				IsXrds = true;
				if (initialResponse != finalResponse) {
					YadisLocation = finalResponse.RequestUri;
				}
			}
		}

		/// <summary>
		/// The URI of the original YADIS discovery request.  
		/// This is the user supplied Identifier as given in the original
		/// YADIS discovery request.
		/// </summary>
		public Uri RequestUri { get; private set; }

		/// <summary>
		/// Gets the fully resolved (after redirects) URL of the user supplied Identifier.
		/// This becomes the ClaimedIdentifier.
		/// </summary>
		public Uri NormalizedUri { get; private set; }

		/// <summary>
		/// Gets the location the XRDS document was downloaded from, if different
		/// from the user supplied Identifier.
		/// </summary>
		public Uri YadisLocation { get; private set; }

		/// <summary>
		/// The Content-Type associated with the <see cref="ResponseText"/>.
		/// </summary>
		public ContentType ContentType { get; private set; }

		/// <summary>
		/// Gets the text in the final response.
		/// This may be an XRDS document or it may be an HTML document, 
		/// as determined by the <see cref="IsXrds"/> property.
		/// </summary>
		public string ResponseText { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the <see cref="ResponseText"/> 
		/// represents an XRDS document. False if the response is an HTML document.
		/// </summary>
		public bool IsXrds { get; private set; }

		/// <summary>
		/// Gets whether discovery resulted in an XRDS document at a referred location.
		/// </summary>
		/// <value><c>true</c> if the response to the userSuppliedIdentifier pointed to a different URL
		/// for the XRDS document.</value>
		public bool UsedYadisLocation {
			get { return YadisLocation != null; }
		}
	}
}
