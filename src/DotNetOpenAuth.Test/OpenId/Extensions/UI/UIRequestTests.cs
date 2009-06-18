//-----------------------------------------------------------------------
// <copyright file="UIRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions.UI {
	using System.Globalization;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Extensions.UI;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class UIRequestTests : OpenIdTestBase {
		[TestMethod]
		public void Defaults() {
			UIRequest request = new UIRequest();
			Assert.AreEqual("popup", request.Mode);
			Assert.AreEqual(CultureInfo.CurrentUICulture, request.LanguagePreference);
		}

		[TestMethod]
		public void LanguagePreferenceEncoding() {
			var request = new UIRequest();
			request.LanguagePreference = new CultureInfo("en-US");
			MessageDictionary dictionary = this.MessageDescriptions.GetAccessor(request);
			Assert.AreEqual("en-US", dictionary["lang"]);
		}

		[TestMethod]
		public void ModeEncoding() {
			var request = new UIRequest();
			MessageDictionary dictionary = this.MessageDescriptions.GetAccessor(request);
			Assert.AreEqual("popup", dictionary["mode"]);
		}
	}
}
