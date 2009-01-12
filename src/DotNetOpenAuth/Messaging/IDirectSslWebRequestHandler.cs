//-----------------------------------------------------------------------
// <copyright file="IDirectSslWebRequestHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.IO;
	using System.Net;

	/// <summary>
	/// A contract for <see cref="HttpWebRequest"/> handling,
	/// with added support for SSL-only requests.
	/// </summary>
	public interface IDirectSslWebRequestHandler : IDirectWebRequestHandler {
		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <param name="requireSsl">if set to <c>true</c> all requests made with this instance must be completed using SSL.</param>
		/// <returns>
		/// The stream the caller should write out the entity data to.
		/// </returns>
		Stream GetRequestStream(HttpWebRequest request, bool requireSsl);

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the 
		/// <see cref="HttpWebResponse"/> to a <see cref="DirectWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <param name="requireSsl">if set to <c>true</c> all requests made with this instance must be completed using SSL.</param>
		/// <returns>An instance of <see cref="DirectWebResponse"/> describing the response.</returns>
		DirectWebResponse GetResponse(HttpWebRequest request, bool requireSsl);
	}
}
