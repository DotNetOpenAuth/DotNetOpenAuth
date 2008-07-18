using System;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// Encodes responses in to <see cref="WebResponse"/>.
	/// </summary>
	internal class Encoder {
		/// <summary>
		/// Encodes responses in to WebResponses.
		/// </summary>
		public virtual Response Encode(IEncodable response) {
			EncodingType encode_as = response.EncodingType;
			Response wr;

			switch (encode_as) {
				case EncodingType.ResponseBody:
					HttpStatusCode code = (response is Exception) ?
						HttpStatusCode.BadRequest : HttpStatusCode.OK;
					wr = new Response(code, null, ProtocolMessages.KeyValueForm.GetBytes(response.EncodedFields));
					break;
				case EncodingType.RedirectBrowserUrl:
					Debug.Assert(response.RedirectUrl != null);
					WebHeaderCollection headers = new WebHeaderCollection();

					UriBuilder builder = new UriBuilder(response.RedirectUrl);
					UriUtil.AppendQueryArgs(builder, response.EncodedFields);
					headers.Add(HttpResponseHeader.Location, builder.Uri.AbsoluteUri);

					wr = new Response(HttpStatusCode.Redirect, headers, new byte[0]);
					break;
				default:
					Logger.ErrorFormat("Cannot encode response: {0}", response);
					wr = new Response(HttpStatusCode.BadRequest, null, new byte[0]);
					break;
			}
			return wr;
		}
	}

	/// <summary>
	/// Encodes responses in to <see cref="WebResponse"/>, signing them when required.
	/// </summary>
	internal class SigningEncoder : Encoder {
		Signatory signatory;

		public SigningEncoder(Signatory signatory) {
			if (signatory == null)
				throw new ArgumentNullException("signatory", "Must have a store to sign this request");

			this.signatory = signatory;
		}

		public override Response Encode(IEncodable encodable) {
			OnSigning(encodable);
			var response = encodable as EncodableResponse;
			if (response != null) {
				if (response.NeedsSigning) {
					Debug.Assert(!response.Fields.ContainsKey(encodable.Protocol.openidnp.sig));
					signatory.Sign(response);
				}
			}
			return base.Encode(encodable);
		}

		/// <summary>
		/// Used for testing.  Allows interception and modification of messages 
		/// that are about to be returned to the RP.
		/// </summary>
		public static event EventHandler<EncodeEventArgs> Signing;
		protected virtual void OnSigning(IEncodable encodable) {
			if (Signing != null)
				Signing(this, new EncodeEventArgs(encodable));
		}
	}

	internal class EncodeEventArgs : EventArgs {
		public EncodeEventArgs(IEncodable encodable) {
			Message = encodable;
		}
		public IEncodable Message { get; private set;}
	}
}
