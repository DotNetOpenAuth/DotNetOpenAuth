//-----------------------------------------------------------------------
// <copyright file="OpenIdProviderTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.Provider;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class OpenIdProviderTests : OpenIdTestBase {
		private OpenIdProvider provider;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.provider = this.CreateProvider();
		}

		/// <summary>
		/// Verifies that the constructor throws an exception if the app store is null.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNull() {
			new OpenIdProvider(null);
		}

		/// <summary>
		/// Verifies that the SecuritySettings property throws when set to null.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SecuritySettingsSetNull() {
			this.provider.SecuritySettings = null;
		}

		/// <summary>
		/// Verifies the SecuritySettings property can be set to a new instance.
		/// </summary>
		[TestMethod]
		public void SecuritySettings() {
			var newSettings = new ProviderSecuritySettings();
			this.provider.SecuritySettings = newSettings;
			Assert.AreSame(newSettings, this.provider.SecuritySettings);
		}

		/// <summary>
		/// Verifies the Channel property.
		/// </summary>
		[TestMethod]
		public void ChannelGetter() {
			Assert.IsNotNull(this.provider.Channel);
		}

		/// <summary>
		/// Verifies the GetRequest method throws outside an HttpContext.
		/// </summary>
		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void GetRequestNoContext() {
			this.provider.GetRequest();
		}
	}
}
