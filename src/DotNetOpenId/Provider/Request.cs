using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Web;

namespace DotNetOpenId.Provider
{
	/// <summary>
	/// Represents any OpenId-protocol request that may come to the provider.
	/// </summary>
	[DebuggerDisplay("Mode: {Mode}, OpenId: {Protocol.Version}")]
	abstract class Request : IRequest {
		protected Request(OpenIdProvider provider) {
			if (provider == null) throw new ArgumentNullException("provider");
			Provider = provider;
			Query = provider.Query;
			Protocol = Protocol.Detect(Query);
			IncomingExtensions = ExtensionArgumentsManager.CreateIncomingExtensions(Query);
			OutgoingExtensions = ExtensionArgumentsManager.CreateOutgoingExtensions(Protocol);
		}

		/// <summary>
		/// The detected protocol of the calling OpenId relying party.
		/// </summary>
		protected internal Protocol Protocol;
		protected IDictionary<string, string> Query { get; private set; }
		protected internal OpenIdProvider Provider { get; private set; }
		internal abstract string Mode { get; }
		/// <summary>
		/// Extension arguments to pass to the Relying Party.
		/// </summary>
		protected ExtensionArgumentsManager OutgoingExtensions { get; private set; }
		/// <summary>
		/// Extension arguments received from the Relying Party.
		/// </summary>
		protected ExtensionArgumentsManager IncomingExtensions { get; private set; }

		/// <summary>
		/// Tests whether a given dictionary represents an incoming OpenId request.
		/// </summary>
		/// <param name="query">The name/value pairs in the querystring or Form submission.  Cannot be null.</param>
		/// <returns>True if the request is an OpenId request, false otherwise.</returns>
		internal static bool IsOpenIdRequest(IDictionary<string, string> query) {
			Debug.Assert(query != null);
			Protocol protocol = Protocol.Detect(query);
			foreach (string key in query.Keys) {
				if (key.StartsWith(protocol.openid.Prefix, StringComparison.OrdinalIgnoreCase)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Creates the appropriate Request-derived type based on the request dictionary.
		/// </summary>
		/// <param name="provider">The Provider instance that called this method.</param>
		/// <returns>A Request-derived type appropriate for this stage in authentication.</returns>
		internal static Request CreateRequest(OpenIdProvider provider) {
			if (provider == null) throw new ArgumentNullException("provider");
			Debug.Assert(provider.Protocol != null, "This should have been set already.");
			string mode = Util.GetRequiredArg(provider.Query, provider.Protocol.openid.mode);

			Request request;
			try {
				if (mode == provider.Protocol.Args.Mode.checkid_setup ||
					mode == provider.Protocol.Args.Mode.checkid_immediate)
					request = new CheckIdRequest(provider);
				else if (mode == provider.Protocol.Args.Mode.check_authentication)
					request = new CheckAuthRequest(provider);
				else if (mode == provider.Protocol.Args.Mode.associate)
					request = new AssociateRequest(provider);
				else
					throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
						Strings.InvalidOpenIdQueryParameterValue, provider.Protocol.openid.mode,
						mode), provider.Query);
			} catch (OpenIdException ex) {
				request = new FaultyRequest(provider, ex);
			}
			return request;
		}

		/// <summary>
		/// Indicates whether this request has all the information necessary to formulate a response.
		/// </summary>
		public abstract bool IsResponseReady { get; }
		internal abstract IEncodable CreateResponse();
		/// <summary>
		/// Called whenever a property changes that would cause the response to need to be
		/// regenerated if it had already been generated.
		/// </summary>
		protected void InvalidateResponse() {
			response = null;
		}
		Response response;
		/// <summary>
		/// The authentication response to be sent to the user agent or the calling
		/// OpenId consumer.
		/// </summary>
		public Response Response {
			get {
				if (!IsResponseReady) throw new InvalidOperationException(Strings.ResponseNotReady);
				if (response == null) {
					var encodableResponse = CreateResponse();
					EncodableResponse extendableResponse = encodableResponse as EncodableResponse;
					if (extendableResponse != null) {
						foreach (var pair in OutgoingExtensions.GetArgumentsToSend(false)) {
							extendableResponse.Fields.Add(pair.Key, pair.Value);
							extendableResponse.Signed.Add(pair.Key);
						}
					}
					response = Provider.Encoder.Encode(encodableResponse);
				}
				return response;
			}
		}
		IResponse IRequest.Response { get { return this.Response; } }

		/// <summary>
		/// Adds query parameters for OpenID extensions to the response directed 
		/// at the OpenID Relying Party.
		/// </summary>
		public void AddExtensionArguments(string extensionTypeUri, IDictionary<string, string> arguments) {
			OutgoingExtensions.AddExtensionArguments(extensionTypeUri, arguments);
		}

		/// <summary>
		/// Gets the key/value pairs for a given OpenID extension 
		/// of a Relying Party's request.
		/// </summary>
		/// <param name="extensionTypeUri">
		/// The Type URI of the OpenID extension whose arguments are being sought.
		/// </param>
		/// <returns>
		/// Returns key/value pairs for this extension.
		/// </returns>
		public IDictionary<string, string> GetExtensionArguments(string extensionTypeUri) {
			return IncomingExtensions.GetExtensionArguments(extensionTypeUri);
		}

		public override string ToString() {
			string returnString = @"Request.Mode = {0}";
			return string.Format(CultureInfo.CurrentCulture, returnString, Mode);
		}
	}
}
