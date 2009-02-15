//-----------------------------------------------------------------------
// <copyright file="IDirectWebRequestHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System.Collections.Generic;
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
		/// Determines whether this instance can support the specified options.
		/// </summary>
		/// <param name="options">The set of options that might be given in a subsequent web request.</param>
		/// <returns>
		/// 	<c>true</c> if this instance can support the specified options; otherwise, <c>false</c>.
		/// </returns>
		bool CanSupport(DirectWebRequestOptions options);

		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <returns>
		/// The stream the caller should write out the entity data to.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>The caller should have set the <see cref="HttpWebRequest.ContentLength"/>
		/// and any other appropriate properties <i>before</i> calling this method.
		/// Callers <i>must</i> close and dispose of the request stream when they are done
		/// writing to it to avoid taking up the connection too long and causing long waits on
		/// subsequent requests.</para>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.</para>
		/// </remarks>
		Stream GetRequestStream(HttpWebRequest request);

		/// <summary>
		/// Prepares an <see cref="HttpWebRequest"/> that contains an POST entity for sending the entity.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <param name="options">The options to apply to this web request.</param>
		/// <returns>
		/// The stream the caller should write out the entity data to.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>The caller should have set the <see cref="HttpWebRequest.ContentLength"/>
		/// and any other appropriate properties <i>before</i> calling this method.
		/// Callers <i>must</i> close and dispose of the request stream when they are done
		/// writing to it to avoid taking up the connection too long and causing long waits on
		/// subsequent requests.</para>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.</para>
		/// </remarks>
		Stream GetRequestStream(HttpWebRequest request, DirectWebRequestOptions options);

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the 
		/// <see cref="HttpWebResponse"/> to a <see cref="DirectWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>An instance of <see cref="DirectWebResponse"/> describing the response.</returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.  The <see cref="WebException.Response"/>
		/// value, if set, shoud be Closed before throwing.</para>
		/// </remarks>
		DirectWebResponse GetResponse(HttpWebRequest request);

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the 
		/// <see cref="HttpWebResponse"/> to a <see cref="DirectWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <param name="options">The options to apply to this web request.</param>
		/// <returns>An instance of <see cref="DirectWebResponse"/> describing the response.</returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.  The <see cref="WebException.Response"/>
		/// value, if set, shoud be Closed before throwing.</para>
		/// </remarks>
		DirectWebResponse GetResponse(HttpWebRequest request, DirectWebRequestOptions options);
	}
}
