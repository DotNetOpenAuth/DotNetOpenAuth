using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Provider {
	public interface IRequest {
		bool IsResponseReady { get; }
		IResponse Response { get; }
		void AddExtensionArguments(string extensionPrefix, IDictionary<string, string> arguments);
		IDictionary<string, string> GetExtensionArguments(string extensionPrefix);
	}
}
