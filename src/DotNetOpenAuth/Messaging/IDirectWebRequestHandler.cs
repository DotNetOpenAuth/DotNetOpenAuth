//-----------------------------------------------------------------------
// <copyright file="IDirectWebRequestHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.IO;
	using System.Net;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A contract for <see cref="HttpWebRequest"/> handling.
	/// </summary>
	/// <remarks>
	/// Implementations of this interface must be thread safe.
	/// </remarks>
	public interface IDirectWebRequestHandler {
		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <returns>
		/// The stream the caller should write out the entity data to.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// <para>The caller should have set the <see cref="HttpWebRequest.ContentLength"/>
		/// and any other appropriate properties <i>before</i> calling this method.</para>
		/// <para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.</para>
		/// </remarks>
		Stream GetRequestStream(HttpWebRequest request);

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the 
		/// <see cref="HttpWebResponse"/> to a <see cref="DirectWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>An instance of <see cref="DirectWebResponse"/> describing the response.</returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.
		/// </remarks>
		DirectWebResponse GetResponse(HttpWebRequest request);
	}
}
