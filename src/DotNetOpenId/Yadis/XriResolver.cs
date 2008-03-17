using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Janrain.Yadis {
	class XriResolver {
		const string proxy = "http://xri.net/{0}?_xrd_r=application/xrds%2Bxml;sep=false";
		public Uri Resolver { get; private set; }

		public XriResolver(string iname) {
			Resolver = new Uri(string.Format(CultureInfo.InvariantCulture, proxy, iname));
		}
	}
}
