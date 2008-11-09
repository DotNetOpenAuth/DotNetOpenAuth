//-----------------------------------------------------------------------
// <copyright file="ScenarioTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ScenarioTests {
		[TestMethod]
		public void FormAssociation() {
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
				},
				op => {
				});
		}
	}
}
