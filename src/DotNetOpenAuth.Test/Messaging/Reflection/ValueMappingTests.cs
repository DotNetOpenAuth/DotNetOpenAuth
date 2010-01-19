//-----------------------------------------------------------------------
// <copyright file="ValueMappingTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging.Reflection {
	using System;
	using DotNetOpenAuth.Messaging.Reflection;
	using NUnit.Framework;

	[TestFixture]
	public class ValueMappingTests {
		[TestCase, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullToString() {
			new ValueMapping(null, str => new object());
		}

		[TestCase, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullToObject() {
			new ValueMapping(obj => obj.ToString(), null);
		}
	}
}
