//-----------------------------------------------------------------------
// <copyright file="ClaimsResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System;
	using System.Globalization;
	using System.IO;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Threading.Tasks;
	using System.Xml.Serialization;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using NUnit.Framework;

	[TestFixture]
	public class ClaimsResponseTests : OpenIdTestBase {
		[Test]
		public void EmptyMailAddress() {
			ClaimsResponse response = new ClaimsResponse(Constants.TypeUris.Standard);
			response.Email = string.Empty;
			Assert.IsNull(response.MailAddress);
		}

		[Test, Ignore("serialization no longer supported")]
		public void BinarySerialization() {
			ClaimsResponse fields = this.GetFilledData();
			MemoryStream ms = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(ms, fields);

			ms.Position = 0;
			ClaimsResponse fields2 = (ClaimsResponse)formatter.Deserialize(ms);
			Assert.AreEqual(fields, fields2);
		}

		[Test, Ignore("serialization no longer supported")]
		public void XmlSerialization() {
			ClaimsResponse fields = this.GetFilledData();
			MemoryStream ms = new MemoryStream();
			XmlSerializer xs = new XmlSerializer(typeof(ClaimsResponse));
			xs.Serialize(ms, fields);

			ms.Position = 0;
			ClaimsResponse fields2 = (ClaimsResponse)xs.Deserialize(ms);
			Assert.AreEqual(fields, fields2);
		}

		[Test]
		public void EqualityTest() {
			ClaimsResponse fields1 = this.GetFilledData();

			Assert.AreNotEqual(fields1, null);
			Assert.AreNotEqual(fields1, "string");

			ClaimsResponse fields2 = this.GetFilledData();
			Assert.AreNotSame(fields1, fields2, "Test sanity check.");
			Assert.AreEqual(fields1, fields2);

			// go through each property and change it slightly and make sure it causes inequality.
			fields2.Email += "q";
			Assert.AreNotEqual(fields1, fields2);
			fields1.Email = fields2.Email;
			Assert.AreEqual(fields1, fields2, "Test sanity check.");
			fields2.BirthDate = DateTime.Now;
			Assert.AreNotEqual(fields1, fields2);
			fields2.BirthDate = fields1.BirthDate;
			Assert.AreEqual(fields1, fields2, "Test sanity check.");
			fields2.Country += "q";
			Assert.AreNotEqual(fields1, fields2);
			fields2.Country = fields1.Country;
			Assert.AreEqual(fields1, fields2, "Test sanity check.");
			fields2.FullName += "q";
			Assert.AreNotEqual(fields1, fields2);
			fields2.FullName = fields1.FullName;
			Assert.AreEqual(fields1, fields2, "Test sanity check.");
			fields2.Gender = Gender.Female;
			Assert.AreNotEqual(fields1, fields2);
			fields2.Gender = fields1.Gender;
			Assert.AreEqual(fields1, fields2, "Test sanity check.");
			fields2.Language = "gb";
			Assert.AreNotEqual(fields1, fields2);
			fields2.Language = fields1.Language;
			Assert.AreEqual(fields1, fields2, "Test sanity check.");
			fields2.Nickname += "q";
			Assert.AreNotEqual(fields1, fields2);
			fields2.Nickname = fields1.Nickname;
			Assert.AreEqual(fields1, fields2, "Test sanity check.");
			fields2.PostalCode += "q";
			Assert.AreNotEqual(fields1, fields2);
			fields2.PostalCode = fields1.PostalCode;
			Assert.AreEqual(fields1, fields2, "Test sanity check.");
			fields2.TimeZone += "q";
			Assert.AreNotEqual(fields1, fields2);
		}

		[Test]
		public void Birthdates() {
			var response = new ClaimsResponse();
			// Verify that they both start out as null
			Assert.IsNull(response.BirthDateRaw);
			Assert.IsFalse(response.BirthDate.HasValue);

			// Verify that null can be set.
			response.BirthDate = null;
			response.BirthDateRaw = null;
			Assert.IsNull(response.BirthDateRaw);
			Assert.IsFalse(response.BirthDate.HasValue);

			// Verify that the strong-typed BirthDate property can be set and that it affects the raw property.
			response.BirthDate = DateTime.Parse("April 4, 1984");
			Assert.AreEqual(4, response.BirthDate.Value.Month);
			Assert.AreEqual("1984-04-04", response.BirthDateRaw);

			// Verify that the raw property can be set with a complete birthdate and that it affects the strong-typed property.
			response.BirthDateRaw = "1998-05-08";
			Assert.AreEqual("1998-05-08", response.BirthDateRaw);
			Assert.AreEqual(DateTime.Parse("May 8, 1998", CultureInfo.InvariantCulture), response.BirthDate);

			// Verify that an partial raw birthdate works, and sets the strong-typed property to null since it cannot be represented.
			response.BirthDateRaw = "2000-00-00";
			Assert.AreEqual("2000-00-00", response.BirthDateRaw);
			Assert.IsFalse(response.BirthDate.HasValue);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void InvalidRawBirthdate() {
			var response = new ClaimsResponse();
			response.BirthDateRaw = "2008";
		}

		[Test]
		public async Task ResponseAlternateTypeUriTests() {
			var request = new ClaimsRequest(Constants.TypeUris.Variant10);
			request.Email = DemandLevel.Require;

			var response = new ClaimsResponse(Constants.TypeUris.Variant10);
			response.Email = "a@b.com";

			await this.RoundtripAsync(Protocol.Default, new[] { request }, new[] { response });
		}

		private ClaimsResponse GetFilledData() {
			return new ClaimsResponse(Constants.TypeUris.Standard) {
				BirthDate = new DateTime(2005, 2, 3),
				Culture = new System.Globalization.CultureInfo("en-US"),
				Email = "a@b.com",
				FullName = "Jimmy buffet",
				Gender = Gender.Male,
				Nickname = "Jimbo",
				PostalCode = "12345",
				TimeZone = "PST",
			};
		}
	}
}
