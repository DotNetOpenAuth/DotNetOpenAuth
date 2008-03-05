using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Specialized;

namespace DotNetOpenId.Provider {
	public interface IResponse {
		HttpStatusCode Code { get; }
		NameValueCollection Headers { get; }
		byte[] Body { get; }
		void Send();
	}
}
