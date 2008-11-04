//-----------------------------------------------------------------------
// <copyright file="IWebRequestHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.OAuth.ChannelElements {
	using System.IO;
	using System.Net;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A contract for <see cref="HttpWebRequest"/> handling.
	/// </summary>
	internal interface IWebRequestHandler {
		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <returns>The writer the caller should write out the entity data to.</returns>
		TextWriter GetRequestStream(HttpWebRequest request);

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the 
		/// <see cref="HttpWebResponse"/> to a <see cref="Response"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>An instance of <see cref="Response"/> describing the response.</returns>
		Response GetResponse(HttpWebRequest request);
	}
}
