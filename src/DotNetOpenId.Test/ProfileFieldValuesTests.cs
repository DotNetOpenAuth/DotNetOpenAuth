/********************************************************
 * Copyright (C) 2007 Andrew Arnott
 * Released under the New BSD License
 * License available here: http://www.opensource.org/licenses/bsd-license.php
 * For news or support on this file: http://blog.nerdbank.net/
 ********************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.IO;
using NUnit.Framework;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class ProfileFieldValuesTests {
		SimpleRegistrationFieldValues getFilledData() {
			return new SimpleRegistrationFieldValues() {
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

		[Test]
		public void BinarySerialization() {
			SimpleRegistrationFieldValues fields = getFilledData();
			MemoryStream ms = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(ms, fields);

			ms.Position = 0;
			SimpleRegistrationFieldValues fields2 = (SimpleRegistrationFieldValues)formatter.Deserialize(ms);
			Assert.AreEqual(fields, fields2);
		}

		[Test]
		public void XmlSerialization() {
			SimpleRegistrationFieldValues fields = getFilledData();
			MemoryStream ms = new MemoryStream();
			XmlSerializer xs = new XmlSerializer(typeof(SimpleRegistrationFieldValues));
			xs.Serialize(ms, fields);

			ms.Position = 0;
			SimpleRegistrationFieldValues fields2 = (SimpleRegistrationFieldValues)xs.Deserialize(ms);
			Assert.AreEqual(fields, fields2);
		}

		[Test]
		public void TestEquals() {
			SimpleRegistrationFieldValues fields1 = getFilledData();

			Assert.AreNotEqual(fields1, null);
			Assert.AreNotEqual(fields1, "string");

			SimpleRegistrationFieldValues fields2 = getFilledData();
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
		public void Empty() {
			Assert.AreEqual(new SimpleRegistrationFieldValues(), SimpleRegistrationFieldValues.Empty);
		}

		//[Test]
		public void AddToResponse() {
			// TODO
		}

		//[Test]
		public void ReadFromResponse() {
			// TODO
		}
	}
}
