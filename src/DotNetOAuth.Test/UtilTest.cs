//-----------------------------------------------------------------------
// <copyright file="UtilTest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class UtilTest {
		private enum SomeFlags : int {
			None = 0,
			Flag1 = 0x1,
			Flag2 = 0x2,
			Flag1and2 = 0x3,
			Flag3 = 0x4,
			All = 0x7,
		}

		[TestMethod]
		public void GetIndividualFlagsTest() {
			Assert.IsFalse(Util.GetIndividualFlags(SomeFlags.None).Any());
			Assert.AreEqual(SomeFlags.Flag1, (SomeFlags)Util.GetIndividualFlags(SomeFlags.Flag1).Single());
			IList<long> flags = Util.GetIndividualFlags(SomeFlags.Flag1and2).ToList();
			Assert.AreEqual(SomeFlags.Flag1, (SomeFlags)flags[0]);
			Assert.AreEqual(SomeFlags.Flag2, (SomeFlags)flags[1]);
			flags = Util.GetIndividualFlags(SomeFlags.All).ToList();
			Assert.AreEqual(SomeFlags.Flag1, (SomeFlags)flags[0]);
			Assert.AreEqual(SomeFlags.Flag2, (SomeFlags)flags[1]);
			Assert.AreEqual(SomeFlags.Flag3, (SomeFlags)flags[2]);
		}
	}
}
