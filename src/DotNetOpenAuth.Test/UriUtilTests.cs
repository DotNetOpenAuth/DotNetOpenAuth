//-----------------------------------------------------------------------
// <copyright file="UriUtilTests.cs" company="Andrew Arnott">
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
	public class UriUtilTests {
		[TestMethod]
		public void QueryStringContainPrefixedParametersNull() {
			Assert.IsFalse(UriUtil.QueryStringContainPrefixedParameters(null, "prefix."));
		}
	}
}
