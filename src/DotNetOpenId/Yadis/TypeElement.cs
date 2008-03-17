using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;

namespace DotNetOpenId.Yadis {
	class TypeElement : XrdsNode {
		public TypeElement(XPathNavigator typeElement, ServiceElement parent) :
			base(typeElement, parent) {
		}

		public string Uri {
			get { return Node.Value; }
		}
	}
}
