//-----------------------------------------------------------------------
// <copyright file="UriHelperTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Test {
	using System;
	using DotNetOpenAuth.AspNet.Clients;
	using NUnit.Framework;

	[TestFixture]
	public class UriHelperTest {
		[TestCase]
		public void TestAttachQueryStringParameterMethod() {
			// Arrange
			string[] input = new string[]
			{
				"http://x.com",
				"https://xxx.com/one?s=123",
				"https://yyy.com/?s=6&u=a",
				"https://zzz.com/default.aspx?name=sd"
			};

			string[] expectedOutput = new string[]
			{
				"http://x.com/?s=awesome",
				"https://xxx.com/one?s=awesome",
				"https://yyy.com/?s=awesome&u=a",
				"https://zzz.com/default.aspx?name=sd&s=awesome"
			};

			for (int i = 0; i < input.Length; i++) {
				// Act
				var inputUrl = new Uri(input[i]);
				var outputUri = UriHelper.AttachQueryStringParameter(inputUrl, "s", "awesome");

				// Assert
				Assert.AreEqual(expectedOutput[i], outputUri.ToString());
			}
		}
	}
}
