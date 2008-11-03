//-----------------------------------------------------------------------
// <copyright file="CommonConsumerBaseTest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.CommonConsumers {
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOAuth.CommonConsumers;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class CommonConsumerBaseTest : TestBase {
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
			Assert.IsFalse(CommonConsumerBase_Accessor.GetIndividualFlags(SomeFlags.None).Any());
			Assert.AreEqual(SomeFlags.Flag1, (SomeFlags)CommonConsumerBase_Accessor.GetIndividualFlags(SomeFlags.Flag1).Single());
			IList<long> flags = CommonConsumerBase_Accessor.GetIndividualFlags(SomeFlags.Flag1and2).ToList();
			Assert.AreEqual(SomeFlags.Flag1, (SomeFlags)flags[0]);
			Assert.AreEqual(SomeFlags.Flag2, (SomeFlags)flags[1]);
			flags = CommonConsumerBase_Accessor.GetIndividualFlags(SomeFlags.All).ToList();
			Assert.AreEqual(SomeFlags.Flag1, (SomeFlags)flags[0]);
			Assert.AreEqual(SomeFlags.Flag2, (SomeFlags)flags[1]);
			Assert.AreEqual(SomeFlags.Flag3, (SomeFlags)flags[2]);
		}
	}
}
