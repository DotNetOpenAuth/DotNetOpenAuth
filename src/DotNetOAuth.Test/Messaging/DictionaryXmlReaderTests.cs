//-----------------------------------------------------------------------
// <copyright file="DictionaryXmlReaderTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Xml.Linq;
	using DotNetOAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class DictionaryXmlReaderTests : TestBase {
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CreateWithNullRootElement() {
			DictionaryXmlReader.Create(null, new Dictionary<string, string>());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CreateWithNullFields() {
			DictionaryXmlReader.Create(XName.Get("name", "ns"), null);
		}
	}
}
