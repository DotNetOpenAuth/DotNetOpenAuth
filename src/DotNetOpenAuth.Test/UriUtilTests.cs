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
	using NUnit.Framework;

	[TestFixture]
	public class UriUtilTests {
		[TestCase]
		public void QueryStringContainPrefixedParametersNull() {
			Assert.IsFalse(UriUtil.QueryStringContainPrefixedParameters(null, "prefix."));
		}
	}
}
