using System;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Diagnostics;
using DotNetOpenId.Provider;
using System.IO;
using System.Web;

namespace DotNetOpenId {
	/// <summary>
	/// Encodes <see cref="IEncodable"/> messages into <see cref="Response"/> instances
	/// that can be interpreted by the host web site.
	/// </summary>
	internal class MessageEncoder {
		/// <summary>
		/// The HTTP Content-Type to use in Key-Value Form responses.
		/// </summary>
		/// <remarks>
		/// OpenID 2.0 section 5.1.2 says this SHOULD be text/plain.  But this value 
		/// does not prevent free hosters like GoDaddy from tacking on their ads
		/// to the end of the direct response, corrupting the data.  So we deviate
		/// from the spec a bit here to improve the story for free Providers.
		/// </remarks>
		const string KeyValueFormContentType = "application/x-openid-kvf";
		/// <summary>
		/// The maximum allowable size for a 301 Redirect response before we send
		/// a 200 OK response with a scripted form POST with the parameters instead
		/// in order to ensure successfully sending a large payload to another server
		/// that might have a maximum allowable size restriction on its GET request.
		/// </summary>
		internal static int GetToPostThreshold = 2 * 1024; // 2KB, recommended by OpenID group
		// We are intentionally using " instead of the html single quote ' below because
		// the HtmlEncode'd values that we inject will only escape the double quote, so
		// only the double-quote used around these values is safe.
		const string FormPostFormat = @"
<html>
<body onload=""var btn = document.getElementById('submit_button'); btn.disabled = true; btn.value = 'Login in progress'; document.getElementById('openid_message').submit()"">
<form id=""openid_message"" action=""{0}"" method=""post"" accept-charset=""UTF-8"" enctype=""application/x-www-form-urlencoded"" onSubmit=""var btn = document.getElementById('submit_button'); btn.disabled = true; btn.value = 'Login in progress'; return true;"">
{1}
	<input id=""submit_button"" type=""submit"" value=""Continue"" />
</form>
</body>
</html>
";
		/// <summary>
		/// Encodes messages into <see cref="Response"/> instances.
		/// </summary>
		public virtual Response Encode(IEncodable message) {
			if (message == null) throw new ArgumentNullException("message");

			EncodingType encode_as = message.EncodingType;
			Response wr;

			WebHeaderCollection headers = new WebHeaderCollection();
			switch (encode_as) {
				case EncodingType.DirectResponse:
					Logger.DebugFormat("Sending direct message response:{0}{1}",
						Environment.NewLine, Util.ToString(message.EncodedFields));
					HttpStatusCode code = (message is Exception) ?
						HttpStatusCode.BadRequest : HttpStatusCode.OK;
					// Key-Value Encoding is how response bodies are sent.
					// Setting the content-type to something other than text/html or text/plain
					// prevents free hosted sites like GoDaddy's from automatically appending
					// the <script/> at the end that adds a banner, and invalidating our response.
					headers.Add(HttpResponseHeader.ContentType, KeyValueFormContentType);
					wr = new Response(code, headers, ProtocolMessages.KeyValueForm.GetBytes(message.EncodedFields), message);
					break;
				case EncodingType.IndirectMessage:
					Logger.DebugFormat("Sending indirect message:{0}{1}",
						Environment.NewLine, Util.ToString(message.EncodedFields));
					// TODO: either redirect or do a form POST depending on payload size.
					Debug.Assert(message.RedirectUrl != null);
					if (getSizeOfPayload(message) <= GetToPostThreshold)
						wr = Create301RedirectResponse(message);
					else
						wr = CreateFormPostResponse(message);
					break;
				default:
					Logger.ErrorFormat("Cannot encode response: {0}", message);
					wr = new Response(HttpStatusCode.BadRequest, headers, new byte[0], message);
					break;
			}
			return wr;
		}

		static int getSizeOfPayload(IEncodable message) {
			Debug.Assert(message != null);
			int size = 0;
			foreach (var field in message.EncodedFields) {
				size += field.Key.Length;
				size += field.Value.Length;
			}
			return size;
		}
		protected virtual Response Create301RedirectResponse(IEncodable message) {
			WebHeaderCollection headers = new WebHeaderCollection();
			UriBuilder builder = new UriBuilder(message.RedirectUrl);
			UriUtil.AppendQueryArgs(builder, message.EncodedFields);
			headers.Add(HttpResponseHeader.Location, builder.Uri.AbsoluteUri);
			Logger.DebugFormat("Redirecting to {0}", builder.Uri.AbsoluteUri);
			return new Response(HttpStatusCode.Redirect, headers, new byte[0], message);
		}
		protected virtual Response CreateFormPostResponse(IEncodable message) {
			WebHeaderCollection headers = new WebHeaderCollection();
			MemoryStream body = new MemoryStream();
			StreamWriter bodyWriter = new StreamWriter(body);
			StringBuilder hiddenFields = new StringBuilder();
			foreach (var field in message.EncodedFields) {
				hiddenFields.AppendFormat("\t<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />\r\n",
					HttpUtility.HtmlEncode(field.Key), HttpUtility.HtmlEncode(field.Value));
			}
			bodyWriter.WriteLine(FormPostFormat,
				HttpUtility.HtmlEncode(message.RedirectUrl.AbsoluteUri), hiddenFields);
			bodyWriter.Flush();
			return new Response(HttpStatusCode.OK, headers, body.ToArray(), message);
		}
	}

	internal class EncodeEventArgs : EventArgs {
		public EncodeEventArgs(IEncodable encodable) {
			Message = encodable;
		}
		public IEncodable Message { get; private set; }
	}
}
