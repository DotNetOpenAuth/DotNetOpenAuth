namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Net;
	using DotNetOpenAuth.Messaging;

	public static class Util {
		/// <summary>
		/// Pseudo-random data generator.
		/// </summary>
		internal static readonly Random NonCryptoRandomDataGenerator = new Random();

		/// <summary>
		/// Sets the channel's outgoing HTTP requests to use default network credentials.
		/// </summary>
		/// <param name="channel">The channel to modify.</param>
		public static void UseDefaultNetworkCredentialsOnOutgoingHttpRequests(this Channel channel)
		{
			Debug.Assert(!(channel.WebRequestHandler is WrappingWebRequestHandler), "Wrapping an already wrapped web request handler.  This is legal, but highly suspect of a bug as you don't want to wrap the same channel repeatedly to apply the same effect.");
			AddOutgoingHttpRequestTransform(channel, http => http.Credentials = CredentialCache.DefaultNetworkCredentials);
		}

		/// <summary>
		/// Adds some action to any outgoing HTTP request on this channel.
		/// </summary>
		/// <param name="channel">The channel's whose outgoing HTTP requests should be modified.</param>
		/// <param name="action">The action to perform on outgoing HTTP requests.</param>
		internal static void AddOutgoingHttpRequestTransform(this Channel channel, Action<HttpWebRequest> action) {
			if (channel == null) {
				throw new ArgumentNullException("channel");
			}

			if (action == null) {
				throw new ArgumentNullException("action");
			}

			channel.WebRequestHandler = new WrappingWebRequestHandler(channel.WebRequestHandler, action);
		}

		/// <summary>
		/// Enumerates through the individual set bits in a flag enum.
		/// </summary>
		/// <param name="flags">The flags enum value.</param>
		/// <returns>An enumeration of just the <i>set</i> bits in the flags enum.</returns>
		internal static IEnumerable<long> GetIndividualFlags(Enum flags) {
			long flagsLong = Convert.ToInt64(flags);
			for (int i = 0; i < sizeof(long) * 8; i++) { // long is the type behind the largest enum
				// Select an individual application from the scopes.
				long individualFlagPosition = (long)Math.Pow(2, i);
				long individualFlag = flagsLong & individualFlagPosition;
				if (individualFlag == individualFlagPosition) {
					yield return individualFlag;
				}
			}
		}

		internal static Uri GetCallbackUrlFromContext() {
			Uri callback = MessagingUtilities.GetRequestUrlFromContext().StripQueryArgumentsWithPrefix("oauth_");
			return callback;
		}

		/// <summary>
		/// Copies the contents of one stream to another.
		/// </summary>
		/// <param name="copyFrom">The stream to copy from, at the position where copying should begin.</param>
		/// <param name="copyTo">The stream to copy to, at the position where bytes should be written.</param>
		/// <returns>The total number of bytes copied.</returns>
		/// <remarks>
		/// Copying begins at the streams' current positions.
		/// The positions are NOT reset after copying is complete.
		/// </remarks>
		internal static int CopyTo(this Stream copyFrom, Stream copyTo) {
			return CopyTo(copyFrom, copyTo, int.MaxValue);
		}

		/// <summary>
		/// Copies the contents of one stream to another.
		/// </summary>
		/// <param name="copyFrom">The stream to copy from, at the position where copying should begin.</param>
		/// <param name="copyTo">The stream to copy to, at the position where bytes should be written.</param>
		/// <param name="maximumBytesToCopy">The maximum bytes to copy.</param>
		/// <returns>The total number of bytes copied.</returns>
		/// <remarks>
		/// Copying begins at the streams' current positions.
		/// The positions are NOT reset after copying is complete.
		/// </remarks>
		internal static int CopyTo(this Stream copyFrom, Stream copyTo, int maximumBytesToCopy) {
			if (copyFrom == null) {
				throw new ArgumentNullException("copyFrom");
			}
			if (copyTo == null) {
				throw new ArgumentNullException("copyTo");
			}

			byte[] buffer = new byte[1024];
			int readBytes;
			int totalCopiedBytes = 0;
			while ((readBytes = copyFrom.Read(buffer, 0, Math.Min(1024, maximumBytesToCopy))) > 0) {
				int writeBytes = Math.Min(maximumBytesToCopy, readBytes);
				copyTo.Write(buffer, 0, writeBytes);
				totalCopiedBytes += writeBytes;
				maximumBytesToCopy -= writeBytes;
			}

			return totalCopiedBytes;
		}

		/// <summary>
		/// Wraps some instance of a web request handler in order to perform some extra operation on all
		/// outgoing HTTP requests.
		/// </summary>
		private class WrappingWebRequestHandler : IDirectWebRequestHandler
		{
			/// <summary>
			/// The handler being wrapped.
			/// </summary>
			private readonly IDirectWebRequestHandler wrappedHandler;

			/// <summary>
			/// The action to perform on outgoing HTTP requests.
			/// </summary>
			private readonly Action<HttpWebRequest> action;

			/// <summary>
			/// Initializes a new instance of the <see cref="WrappingWebRequestHandler"/> class.
			/// </summary>
			/// <param name="wrappedHandler">The HTTP handler to wrap.</param>
			/// <param name="action">The action to perform on outgoing HTTP requests.</param>
			internal WrappingWebRequestHandler(IDirectWebRequestHandler wrappedHandler, Action<HttpWebRequest> action)
			{
				if (wrappedHandler == null) {
					throw new ArgumentNullException("wrappedHandler");
				}

				if (action == null) {
					throw new ArgumentNullException("action");
				}

				this.wrappedHandler = wrappedHandler;
				this.action = action;
			}

			#region Implementation of IDirectWebRequestHandler

			/// <summary>
			/// Determines whether this instance can support the specified options.
			/// </summary>
			/// <param name="options">The set of options that might be given in a subsequent web request.</param>
			/// <returns>
			/// 	<c>true</c> if this instance can support the specified options; otherwise, <c>false</c>.
			/// </returns>
			public bool CanSupport(DirectWebRequestOptions options)
			{
				return this.wrappedHandler.CanSupport(options);
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
			public Stream GetRequestStream(HttpWebRequest request)
			{
				this.action(request);
				return this.wrappedHandler.GetRequestStream(request);
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
			public Stream GetRequestStream(HttpWebRequest request, DirectWebRequestOptions options)
			{
				this.action(request);
				return this.wrappedHandler.GetRequestStream(request, options);
			}

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
			public IncomingWebResponse GetResponse(HttpWebRequest request)
			{
				// If the request has an entity, the action would have already been processed in GetRequestStream.
				if (request.Method == "GET")
				{
					this.action(request);
				}

				return this.wrappedHandler.GetResponse(request);
			}

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
			public IncomingWebResponse GetResponse(HttpWebRequest request, DirectWebRequestOptions options)
			{
				// If the request has an entity, the action would have already been processed in GetRequestStream.
				if (request.Method == "GET") {
					this.action(request);
				}

				return this.wrappedHandler.GetResponse(request, options);
			}

			#endregion
		}
	}
}
