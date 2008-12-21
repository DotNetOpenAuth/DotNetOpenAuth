//-----------------------------------------------------------------------
// <copyright file="ClaimsResponseTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Xml.Serialization;
	using System.IO;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

	[TestClass]
	public class ClaimsResponseTests {
		private ClaimsResponse getFilledData() {
			return new ClaimsResponse(Constants.sreg_ns) {
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

		[TestMethod]
		public void EmptyMailAddress() {
			ClaimsResponse response = new ClaimsResponse(Constants.sreg_ns);
			response.Email = "";
			Assert.IsNull(response.MailAddress);
		}

		[TestMethod]
		public void BinarySerialization() {
			ClaimsResponse fields = getFilledData();
			MemoryStream ms = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(ms, fields);

			ms.Position = 0;
			ClaimsResponse fields2 = (ClaimsResponse)formatter.Deserialize(ms);
			Assert.AreEqual(fields, fields2);
		}

		[TestMethod]
		public void XmlSerialization() {
			ClaimsResponse fields = getFilledData();
			MemoryStream ms = new MemoryStream();
			XmlSerializer xs = new XmlSerializer(typeof(ClaimsResponse));
			xs.Serialize(ms, fields);

			ms.Position = 0;
			ClaimsResponse fields2 = (ClaimsResponse)xs.Deserialize(ms);
			Assert.AreEqual(fields, fields2);
		}

		[TestMethod]
		public void TestEquals() {
			ClaimsResponse fields1 = getFilledData();

			Assert.AreNotEqual(fields1, null);
			Assert.AreNotEqual(fields1, "string");

			ClaimsResponse fields2 = getFilledData();
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

		void parameterizedPreserveVersionFromRequest(string versionTypeUri) {
			Dictionary<string, string> fields = new Dictionary<string, string>{
				{"optional", "nickname"},
			};
			var req = new ClaimsRequest();
			Assert.IsTrue(((IExtensionRequest)req).Deserialize(fields, null, versionTypeUri));
			Assert.AreEqual(DemandLevel.Request, req.Nickname);
			ClaimsResponse resp = req.CreateResponse();
			Assert.AreEqual(versionTypeUri, ((IExtensionResponse)resp).TypeUri);
		}

		[TestMethod]
		public void PreserveVersionFromRequest() {
			// some unofficial type URIs...
			parameterizedPreserveVersionFromRequest("http://openid.net/sreg/1.0");
			parameterizedPreserveVersionFromRequest("http://openid.net/sreg/1.1");
			// and the official one.
			parameterizedPreserveVersionFromRequest("http://openid.net/extensions/sreg/1.1");
		}

		//[TestMethod]
		public void AddToResponse() {
			// TODO
		}

		//[TestMethod]
		public void ReadFromResponse() {
			// TODO
		}
	}
}
