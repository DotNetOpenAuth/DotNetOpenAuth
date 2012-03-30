//-----------------------------------------------------------------------
// <copyright file="UIRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions.UI {
	using System.Globalization;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Extensions.UI;
	using NUnit.Framework;

	[TestFixture]
	public class UIRequestTests : OpenIdTestBase {
		[Test]
		public void Defaults() {
			var request = new UIRequest();
			Assert.AreEqual("popup", request.Mode);
			Assert.AreEqual(1, request.LanguagePreference.Length);
			Assert.AreEqual(CultureInfo.CurrentUICulture, request.LanguagePreference[0]);
			Assert.IsFalse(request.Icon.HasValue);
		}

		[Test]
		public void IconEncodingDecoding()
		{
			var request = new UIRequest();
			MessageDictionary dictionary = this.MessageDescriptions.GetAccessor(request);
			Assert.IsFalse(dictionary.ContainsKey("icon"));

			Assert.IsFalse(request.Icon.HasValue);
			dictionary["icon"] = "true";
			Assert.IsTrue(request.Icon.Value);

			dictionary.ClearValues();
			request.Icon = true;
			Assert.AreEqual("true", dictionary["icon"]);
		}

		[Test]
		public void LanguagePreferenceEncodingDecoding() {
			var request = new UIRequest();
			MessageDictionary dictionary = this.MessageDescriptions.GetAccessor(request);

			request.LanguagePreference = new[] { new CultureInfo("en-US") };
			Assert.AreEqual("en-US", dictionary["lang"]);

			request.LanguagePreference = new[] { new CultureInfo("en-US"), new CultureInfo("es-ES") };
			Assert.AreEqual("en-US,es-ES", dictionary["lang"]);

			// Now test decoding
			dictionary["lang"] = "en-US";
			Assert.AreEqual(1, request.LanguagePreference.Length);
			Assert.AreEqual(new CultureInfo("en-US"), request.LanguagePreference[0]);

			dictionary["lang"] = "en-US,es-ES";
			Assert.AreEqual(2, request.LanguagePreference.Length);
			Assert.AreEqual(new CultureInfo("en-US"), request.LanguagePreference[0]);
			Assert.AreEqual(new CultureInfo("es-ES"), request.LanguagePreference[1]);
		}

		[Test]
		public void ModeEncoding() {
			var request = new UIRequest();
			MessageDictionary dictionary = this.MessageDescriptions.GetAccessor(request);
			Assert.AreEqual("popup", dictionary["mode"]);
		}
	}
}
