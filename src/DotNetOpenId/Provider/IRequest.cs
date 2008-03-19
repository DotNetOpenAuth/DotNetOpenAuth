using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Provider {
	public interface IRequest {
		bool IsResponseReady { get; }
		IResponse Response { get; }
		void AddExtensionArguments(string extensionTypeUri, IDictionary<string, string> arguments);
		IDictionary<string, string> GetExtensionArguments(string extensionTypeUri);
	}
}
