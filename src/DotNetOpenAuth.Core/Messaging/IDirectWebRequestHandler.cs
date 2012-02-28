﻿//-----------------------------------------------------------------------
// <copyright file="IDirectWebRequestHandler.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Net;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A contract for <see cref="HttpWebRequest"/> handling.
	/// </summary>
	/// <remarks>
	/// Implementations of this interface must be thread safe.
	/// </remarks>
	[ContractClass(typeof(IDirectWebRequestHandlerContract))]
	public interface IDirectWebRequestHandler {
		/// <summary>
		/// Determines whether this instance can support the specified options.
		/// </summary>
		/// <param name="options">The set of options that might be given in a subsequent web request.</param>
		/// <returns>
		/// 	<c>true</c> if this instance can support the specified options; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
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
		/// <see cref="HttpWebResponse"/> to a <see cref="IncomingWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>An instance of <see cref="IncomingWebResponse"/> describing the response.</returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.  The <see cref="WebException.Response"/>
		/// value, if set, should be Closed before throwing.</para>
		/// </remarks>
		IncomingWebResponse GetResponse(HttpWebRequest request);

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the 
		/// <see cref="HttpWebResponse"/> to a <see cref="IncomingWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <param name="options">The options to apply to this web request.</param>
		/// <returns>An instance of <see cref="IncomingWebResponse"/> describing the response.</returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// 	<para>Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.  The <see cref="WebException.Response"/>
		/// value, if set, should be Closed before throwing.</para>
		/// </remarks>
		IncomingWebResponse GetResponse(HttpWebRequest request, DirectWebRequestOptions options);
	}

	/// <summary>
	/// Code contract for the <see cref="IDirectWebRequestHandler"/> type.
	/// </summary>
	[ContractClassFor(typeof(IDirectWebRequestHandler))]
	internal abstract class IDirectWebRequestHandlerContract : IDirectWebRequestHandler {
		#region IDirectWebRequestHandler Members

		/// <summary>
		/// Determines whether this instance can support the specified options.
		/// </summary>
		/// <param name="options">The set of options that might be given in a subsequent web request.</param>
		/// <returns>
		/// 	<c>true</c> if this instance can support the specified options; otherwise, <c>false</c>.
		/// </returns>
		bool IDirectWebRequestHandler.CanSupport(DirectWebRequestOptions options) {
			throw new System.NotImplementedException();
		}

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
		Stream IDirectWebRequestHandler.GetRequestStream(HttpWebRequest request) {
			Requires.NotNull(request, "request");
			throw new System.NotImplementedException();
		}

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
		Stream IDirectWebRequestHandler.GetRequestStream(HttpWebRequest request, DirectWebRequestOptions options) {
			Requires.NotNull(request, "request");
			Requires.Support(((IDirectWebRequestHandler)this).CanSupport(options), MessagingStrings.DirectWebRequestOptionsNotSupported);
			////ErrorUtilities.VerifySupported(((IDirectWebRequestHandler)this).CanSupport(options), string.Format(MessagingStrings.DirectWebRequestOptionsNotSupported, options, this.GetType().Name));
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the
		/// <see cref="HttpWebResponse"/> to a <see cref="IncomingWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>
		/// An instance of <see cref="IncomingWebResponse"/> describing the response.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.  The <see cref="WebException.Response"/>
		/// value, if set, should be Closed before throwing.
		/// </remarks>
		IncomingWebResponse IDirectWebRequestHandler.GetResponse(HttpWebRequest request) {
			Requires.NotNull(request, "request");
			Contract.Ensures(Contract.Result<IncomingWebResponse>() != null);
			Contract.Ensures(Contract.Result<IncomingWebResponse>().ResponseStream != null);
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the
		/// <see cref="HttpWebResponse"/> to a <see cref="IncomingWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <param name="options">The options to apply to this web request.</param>
		/// <returns>
		/// An instance of <see cref="IncomingWebResponse"/> describing the response.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown for any network error.</exception>
		/// <remarks>
		/// Implementations should catch <see cref="WebException"/> and wrap it in a
		/// <see cref="ProtocolException"/> to abstract away the transport and provide
		/// a single exception type for hosts to catch.  The <see cref="WebException.Response"/>
		/// value, if set, should be Closed before throwing.
		/// </remarks>
		IncomingWebResponse IDirectWebRequestHandler.GetResponse(HttpWebRequest request, DirectWebRequestOptions options) {
			Requires.NotNull(request, "request");
			Contract.Ensures(Contract.Result<IncomingWebResponse>() != null);
			Contract.Ensures(Contract.Result<IncomingWebResponse>().ResponseStream != null);
			Requires.Support(((IDirectWebRequestHandler)this).CanSupport(options), MessagingStrings.DirectWebRequestOptionsNotSupported);

			////ErrorUtilities.VerifySupported(((IDirectWebRequestHandler)this).CanSupport(options), string.Format(MessagingStrings.DirectWebRequestOptionsNotSupported, options, this.GetType().Name));
			throw new System.NotImplementedException();
		}

		#endregion
	}
}
