//-----------------------------------------------------------------------
// <copyright file="SigningBindingElementTests .cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Provider;

	[TestClass]
	public class SigningBindingElementTests : OpenIdTestBase {
		/// <summary>
		/// Verifies that all parameters in ExtraData in signed responses are signed.
		/// </summary>
		[TestMethod]
		public void SignedResponsesIncludeExtraDataInSignature() {
			Protocol protocol = Protocol.Default;
			SigningBindingElement sbe = new SigningBindingElement(new AssociationMemoryStore<AssociationRelyingPartyType>(), new ProviderSecuritySettings());
			IndirectSignedResponse response = new IndirectSignedResponse(protocol.Version, RPUri);
			response.ReturnTo = RPUri;
			response.ProviderEndpoint = ProviderUri;

			response.ExtraData["someunsigned"] = "value";
			response.ExtraData["openid.somesigned"] = "value";

			Assert.IsTrue(sbe.PrepareMessageForSending(response));
			ITamperResistantOpenIdMessage signedResponse = (ITamperResistantOpenIdMessage)response;
			
			// Make sure that the extra parameters are signed.
			// Since the signing algorithm only allows for signing parameters that start with
			// 'openid.', other parameters should not be signed.
			Assert.IsNotNull(signedResponse.SignedParameterOrder);
			string[] signedParameters = signedResponse.SignedParameterOrder.Split(',');
			Assert.IsTrue(signedParameters.Contains("somesigned"));
			Assert.IsFalse(signedParameters.Contains("someunsigned"));
		}
	}
}
