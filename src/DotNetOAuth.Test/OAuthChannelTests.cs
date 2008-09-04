//-----------------------------------------------------------------------
// <copyright file="OAuthChannelTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using DotNetOAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class OAuthChannelTests : TestBase {
		private Channel channel;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.channel = new OAuthChannel();
		}

		[TestMethod, Ignore]
		public void ReadFromRequestAuthorization() {
		}
	
		internal static string CreateAuthorizationHeader(IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			StringBuilder authorization = new StringBuilder();
			authorization.Append("OAuth ");
			foreach (var pair in fields) {
				string key = Uri.EscapeDataString(pair.Key);
				string value = Uri.EscapeDataString(pair.Value);
				authorization.Append(key);
				authorization.Append("=\"");
				authorization.Append(value);
				authorization.Append("\",");
			}
			authorization.Length--; // remove trailing comma

			return authorization.ToString();
		}
	}
}
