//-----------------------------------------------------------------------
// <copyright file="UtilTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class UtilTests {
		/// <summary>
		/// Verifies ToStringDeferred generates a reasonable string for an empty, multi-line list.
		/// </summary>
		[TestMethod]
		public void ToStringDeferredEmptyMultiLine() {
			Assert.AreEqual("[]", Util.ToStringDeferred(Enumerable.Empty<string>(), true).ToString());
		}
	}
}
