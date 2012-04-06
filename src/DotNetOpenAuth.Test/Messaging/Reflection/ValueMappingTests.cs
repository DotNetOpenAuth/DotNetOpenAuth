//-----------------------------------------------------------------------
// <copyright file="ValueMappingTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging.Reflection {
	using System;
	using DotNetOpenAuth.Messaging.Reflection;
	using NUnit.Framework;

	[TestFixture]
	public class ValueMappingTests {
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullToString() {
			new ValueMapping(null, null, str => new object());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullToObject() {
			new ValueMapping(obj => obj.ToString(), null, null);
		}
	}
}
