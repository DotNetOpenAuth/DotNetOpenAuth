//-----------------------------------------------------------------------
// <copyright file="Request.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// Implements the <see cref="IRequest" /> interface for all incoming
	/// request messages to an OpenID Provider.
	/// </summary>
	[Serializable]
	internal abstract class Request : IRequest {
		/// <summary>
		/// The incoming request message.
		/// </summary>
		private readonly IDirectedProtocolMessage request;

		/// <summary>
		/// The incoming request message cast to its extensible form.  
		/// Or null if the message does not support extensions.
		/// </summary>
		private readonly IProtocolMessageWithExtensions extensibleMessage;

		/// <summary>
		/// The version of the OpenID protocol to use.
		/// </summary>
		private readonly Version protocolVersion;

		/// <summary>
		/// Backing store for the <see cref="Protocol"/> property.
		/// </summary>
		[NonSerialized]
		private Protocol protocol;

		/// <summary>
		/// The list of extensions to add to the response message.
		/// </summary>
		private List<IOpenIdMessageExtension> responseExtensions = new List<IOpenIdMessageExtension>();

		/// <summary>
		/// Initializes a new instance of the <see cref="Request"/> class.
		/// </summary>
		/// <param name="request">The incoming request message.</param>
		/// <param name="securitySettings">The security settings from the channel.</param>
		protected Request(IDirectedProtocolMessage request, ProviderSecuritySettings securitySettings) {
			Requires.NotNull(request, "request");
			Requires.NotNull(securitySettings, "securitySettings");

			this.request = request;
			this.SecuritySettings = securitySettings;
			this.protocolVersion = this.request.Version;
			this.extensibleMessage = request as IProtocolMessageWithExtensions;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Request"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="securitySettings">The security settings.</param>
		protected Request(Version version, ProviderSecuritySettings securitySettings) {
			Requires.NotNull(version, "version");
			Requires.NotNull(securitySettings, "securitySettings");

			this.protocolVersion = version;
			this.SecuritySettings = securitySettings;
		}

		#region IRequest Properties

		/// <summary>
		/// Gets a value indicating whether the response is ready to be sent to the user agent.
		/// </summary>
		/// <remarks>
		/// This property returns false if there are properties that must be set on this
		/// request instance before the response can be sent.
		/// </remarks>
		public abstract bool IsResponseReady { get; }

		/// <summary>
		/// Gets or sets the security settings that apply to this request.
		/// </summary>
		/// <value>Defaults to the <see cref="OpenIdProvider.SecuritySettings"/> on the <see cref="OpenIdProvider"/>.</value>
		public ProviderSecuritySettings SecuritySettings { get; set; }

		#endregion

		/// <summary>
		/// Gets the original request message.
		/// </summary>
		/// <value>This may be null in the case of an unrecognizable message.</value>
		protected internal IDirectedProtocolMessage RequestMessage {
			get { return this.request; }
		}

		/// <summary>
		/// Gets the protocol version used in the request.
		/// </summary>
		protected Protocol Protocol {
			get {
				if (this.protocol == null) {
					this.protocol = Protocol.Lookup(this.protocolVersion);
				}

				return this.protocol;
			}
		}

		#region IRequest Methods

		/// <summary>
		/// Adds an extension to the response to send to the relying party.
		/// </summary>
		/// <param name="extension">The extension to add to the response message.</param>
		public void AddResponseExtension(IOpenIdMessageExtension extension) {
			// Because the derived AuthenticationRequest class can swap out
			// one response message for another (auth vs. no-auth), and because
			// some response messages support extensions while others don't,
			// we just add the extensions to a collection here and add them 
			// to the response on the way out.
			this.responseExtensions.Add(extension);
		}

		/// <summary>
		/// Removes any response extensions previously added using <see cref="AddResponseExtension"/>.
		/// </summary>
		/// <remarks>
		/// This should be called before sending a negative response back to the relying party
		/// if extensions were already added, since negative responses cannot carry extensions.
		/// </remarks>
		public void ClearResponseExtensions() {
			this.responseExtensions.Clear();
		}

		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <typeparam name="T">The type of the extension.</typeparam>
		/// <returns>
		/// An instance of the extension initialized with values passed in with the request.
		/// </returns>
		public T GetExtension<T>() where T : IOpenIdMessageExtension, new() {
			if (this.extensibleMessage != null) {
				return this.extensibleMessage.Extensions.OfType<T>().SingleOrDefault();
			} else {
				return default(T);
			}
		}

		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <param name="extensionType">The type of the extension.</param>
		/// <returns>
		/// An instance of the extension initialized with values passed in with the request.
		/// </returns>
		public IOpenIdMessageExtension GetExtension(Type extensionType) {
			if (this.extensibleMessage != null) {
				return this.extensibleMessage.Extensions.OfType<IOpenIdMessageExtension>().Where(ext => extensionType.IsInstanceOfType(ext)).SingleOrDefault();
			} else {
				return null;
			}
		}

		#endregion

		/// <summary>
		/// Gets the response to send to the user agent.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The response.</returns>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="IsResponseReady" /> is <c>false</c>.</exception>
		internal async Task<IProtocolMessage> GetResponseAsync(CancellationToken cancellationToken) {
			RequiresEx.ValidState(this.IsResponseReady, OpenIdStrings.ResponseNotReady);

			if (this.responseExtensions.Count > 0) {
				var responseMessage = await this.GetResponseMessageAsync(cancellationToken);
				var extensibleResponse = responseMessage as IProtocolMessageWithExtensions;
				ErrorUtilities.VerifyOperation(extensibleResponse != null, MessagingStrings.MessageNotExtensible, responseMessage.GetType().Name);
				foreach (var extension in this.responseExtensions) {
					// It's possible that a prior call to this property
					// has already added some/all of the extensions to the message.
					// We don't have to worry about deleting old ones because
					// this class provides no facility for removing extensions
					// that are previously added.
					if (!extensibleResponse.Extensions.Contains(extension)) {
						extensibleResponse.Extensions.Add(extension);
					}
				}
			}

			return await this.GetResponseMessageAsync(cancellationToken);
		}

		/// <summary>
		/// Gets the response message, once <see cref="IsResponseReady" /> is <c>true</c>.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The response message.</returns>
		protected abstract Task<IProtocolMessage> GetResponseMessageAsync(CancellationToken cancellationToken);
	}
}
