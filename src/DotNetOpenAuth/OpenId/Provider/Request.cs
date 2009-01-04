//-----------------------------------------------------------------------
// <copyright file="Request.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	internal abstract class Request : IRequest {
		private readonly OpenIdProvider provider;
		private readonly IDirectedProtocolMessage request;
		private readonly IProtocolMessageWithExtensions extensibleMessage;
		private List<IOpenIdMessageExtension> extensions = new List<IOpenIdMessageExtension>();
		private UserAgentResponse cachedUserAgentResponse;

		protected Request(OpenIdProvider provider, IDirectedProtocolMessage request) {
			ErrorUtilities.VerifyArgumentNotNull(provider, "provider");
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			this.provider = provider;
			this.request = request;
			this.extensibleMessage = request as IProtocolMessageWithExtensions;
		}

		#region IRequest Members

		public abstract bool IsResponseReady { get; }

		public UserAgentResponse Response {
			get {
				if (this.cachedUserAgentResponse == null && this.IsResponseReady) {
					if (this.extensions.Count > 0) {
						var extensibleResponse = this.ResponseMessage as IProtocolMessageWithExtensions;
						ErrorUtilities.VerifyOperation(extensibleResponse != null, MessagingStrings.MessageNotExtensible, this.ResponseMessage.GetType().Name);
						foreach (var extension in this.extensions) {
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

					this.cachedUserAgentResponse = this.provider.Channel.Send(this.ResponseMessage);
				}

				return this.cachedUserAgentResponse;
			}
		}

		#endregion

		protected OpenIdProvider Provider {
			get { return this.provider; }
		}

		protected IDirectedProtocolMessage RequestMessage {
			get { return this.request; }
		}

		protected abstract IProtocolMessage ResponseMessage { get; }

		protected Protocol Protocol {
			get { return Protocol.Lookup(this.RequestMessage.Version); }
		}

		#region IRequest Methods

		public void AddResponseExtension(IOpenIdMessageExtension extension) {
			ErrorUtilities.VerifyArgumentNotNull(extension, "extension");

			// Because the derived AuthenticationRequest class can swap out
			// one response message for another (auth vs. no-auth), and because
			// some response messages support extensions while others don't,
			// we just add the extensions to a collection here and add them 
			// to the response on the way out.
			this.extensions.Add(extension);
			this.ResetUserAgentResponse();
		}

		public T GetExtension<T>() where T : IOpenIdMessageExtension, new() {
			if (this.extensibleMessage != null) {
				return this.extensibleMessage.Extensions.OfType<T>().SingleOrDefault();
			} else {
				return default(T);
			}
		}

		public IOpenIdMessageExtension GetExtension(Type extensionType) {
			ErrorUtilities.VerifyArgumentNotNull(extensionType, "extensionType");
			if (this.extensibleMessage != null) {
				return this.extensibleMessage.Extensions.OfType<IOpenIdMessageExtension>().Where(ext => extensionType.IsInstanceOfType(ext)).SingleOrDefault();
			} else {
				return null;
			}
		}

		#endregion

		/// <summary>
		/// Resets any user agent response that may have been created already and cached.
		/// </summary>
		protected void ResetUserAgentResponse() {
			this.cachedUserAgentResponse = null;
		}
	}
}
