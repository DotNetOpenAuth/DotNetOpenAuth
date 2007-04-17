using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.IO;
using NUnit.Framework;
using Janrain.OpenId.RegistrationExtension;

namespace OpenIdTests
{
	[TestFixture]
	public class OpenIdProfileFieldsTestSuite
	{
		OpenIdProfileFields getFilledStruct()
		{
			OpenIdProfileFields fields = new OpenIdProfileFields();
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
			OpenIdProfileFields fields = getFilledStruct();
			MemoryStream ms = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(ms, fields);

			ms.Position = 0;
			OpenIdProfileFields fields2 = (OpenIdProfileFields)formatter.Deserialize(ms);
			Assert.AreEqual(fields, fields2);
		}

		[Test]
		public void TestXmlSerialization()
		{
			OpenIdProfileFields fields = getFilledStruct();
			MemoryStream ms = new MemoryStream();
			XmlSerializer xs = new XmlSerializer(typeof(OpenIdProfileFields));
			xs.Serialize(ms, fields);

			ms.Position = 0;
			OpenIdProfileFields fields2 = (OpenIdProfileFields)xs.Deserialize(ms);
			Assert.AreEqual(fields, fields2);
		}
	}
}
