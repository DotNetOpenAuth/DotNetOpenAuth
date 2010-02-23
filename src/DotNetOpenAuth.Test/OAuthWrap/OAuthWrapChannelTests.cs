//-----------------------------------------------------------------------
// <copyright file="OAuthWrapChannelTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;
	using NUnit.Framework;

	[TestFixture]
	public class OAuthWrapChannelTests : OAuthWrapTestBase {
		private OAuthWrapChannel channel;

		public override void SetUp() {
			base.SetUp();

			this.channel = new OAuthWrapChannel();
		}

		/// <summary>
		/// Verifies that the WRAP message types are initialized.
		/// </summary>
		[TestCase]
		public void MessageFactory() {
			// TODO: code here
		}
	}
}
