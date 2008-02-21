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
using DotNetOpenId.RegistrationExtension;

namespace DotNetOpenId.Test
{
	[TestFixture]
	public class OpenIdProfileFieldsTestSuite
	{
		ProfileFieldValues getFilledStruct()
		{
			ProfileFieldValues fields = new ProfileFieldValues();
			fields.Birthdate = new DateTime(2005, 2, 3);
			fields.Culture = new System.Globalization.CultureInfo("en-US");
			fields.Email = "a@b.com";
			fields.Fullname = "Jimmy buffet";
			fields.Gender = Gender.Male;
			fields.Nickname = "Jimbo";
			fields.PostalCode = "12345";
			fields.TimeZone = "PST";
			return fields;
		}

		[Test]
		public void TestBinarySerialization()
		{
			ProfileFieldValues fields = getFilledStruct();
			MemoryStream ms = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(ms, fields);

			ms.Position = 0;
			ProfileFieldValues fields2 = (ProfileFieldValues)formatter.Deserialize(ms);
			Assert.AreEqual(fields, fields2);
		}

		[Test]
		public void TestXmlSerialization()
		{
			ProfileFieldValues fields = getFilledStruct();
			MemoryStream ms = new MemoryStream();
			XmlSerializer xs = new XmlSerializer(typeof(ProfileFieldValues));
			xs.Serialize(ms, fields);

			ms.Position = 0;
			ProfileFieldValues fields2 = (ProfileFieldValues)xs.Deserialize(ms);
			Assert.AreEqual(fields, fields2);
		}
	}
}
