//-----------------------------------------------------------------------
// <copyright file="UriUtilTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
		[Test]
		public void QueryStringContainPrefixedParametersNull() {
			Assert.IsFalse(UriUtil.QueryStringContainPrefixedParameters(null, "prefix."));
		}
	}
}
