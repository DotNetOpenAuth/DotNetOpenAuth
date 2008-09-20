namespace DotNetOAuth.Test.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOAuth.Messaging.Reflection;

	[TestClass]
	public class ValueMappingTests {
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullToString() {
			new ValueMapping(null, str => new object());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullToObject() {
			new ValueMapping(obj => obj.ToString(), null);
		}
	}
}
