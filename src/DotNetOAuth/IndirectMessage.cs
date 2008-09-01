namespace DotNetOAuth {
	using System.Net;

	public class IndirectMessage {
		public WebHeaderCollection Headers { get; set; }

		public byte[] Body { get; set; }

		public HttpStatusCode Status { get; set; }

		internal IProtocolMessage OriginalMessage { get; set; }

		/// <summary>
		/// Requires a current HttpContext.
		/// </summary>
		public void Send() {
			throw new System.NotImplementedException();
		}
	}
}
