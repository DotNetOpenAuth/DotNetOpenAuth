//-----------------------------------------------------------------------
// <copyright file="OpenIdChannelTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class OpenIdChannelTests : TestBase {
		private OpenIdChannel channel;

		[TestInitialize]
		public void Setup() {
			this.channel = new OpenIdChannel();
		}

		[TestMethod]
		public void Ctor() {
		}
	}
}
