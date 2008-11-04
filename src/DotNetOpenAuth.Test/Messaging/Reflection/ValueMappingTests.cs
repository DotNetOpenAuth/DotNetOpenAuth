//-----------------------------------------------------------------------
// <copyright file="ValueMappingTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging.Reflection {
	using System;
	using DotNetOpenAuth.Messaging.Reflection;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

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
