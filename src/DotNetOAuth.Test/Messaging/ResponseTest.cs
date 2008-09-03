//-----------------------------------------------------------------------
// <copyright file="ResponseTest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Messaging {
	using System;
	using DotNetOAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ResponseTest : TestBase {
		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void SendWithoutAspNetContext() {
			new Response().Send();
		}
	}
}
