//-----------------------------------------------------------------------
// <copyright file="AuthenticationTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AuthenticationTests : OpenIdTestBase {
		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		[TestMethod]
		public void SharedAssociationPositive() {
			this.ParameterizedPositiveAuthenticationTest(true, true, false);
		}

		/// <summary>
		/// Verifies that a shared association protects against tampering.
		/// </summary>
		[TestMethod]
		public void SharedAssociationTampered() {
			this.ParameterizedPositiveAuthenticationTest(true, true, true);
		}

		[TestMethod]
		public void SharedAssociationNegative() {
			this.ParameterizedPositiveAuthenticationTest(true, false, false);
		}

		[TestMethod]
		public void PrivateAssociationPositive() {
			this.ParameterizedPositiveAuthenticationTest(false, true, false);
		}

		/// <summary>
		/// Verifies that a private association protects against tampering.
		/// </summary>
		[TestMethod]
		public void PrivateAssociationTampered() {
			this.ParameterizedPositiveAuthenticationTest(false, true, true);
		}

		[TestMethod]
		public void NoAssociationNegative() {
			this.ParameterizedPositiveAuthenticationTest(false, false, false);
		}

		private void ParameterizedPositiveAuthenticationTest(bool sharedAssociation, bool positive, bool tamper) {
			foreach (Protocol protocol in Protocol.AllPracticalVersions) {
				this.ParameterizedPositiveAuthenticationTest(protocol, sharedAssociation, positive, tamper);
			}
		}

		private void ParameterizedPositiveAuthenticationTest(Protocol protocol, bool sharedAssociation, bool positive, bool tamper) {
			ErrorUtilities.VerifyArgument(positive || !tamper, "Cannot tamper with a negative response.");
			Uri userSetupUrl = protocol.Version.Major < 2 ? new Uri("http://usersetupurl") : null;
			Association association = sharedAssociation ? HmacShaAssociation.Create(protocol, protocol.Args.SignatureAlgorithm.Best, AssociationRelyingPartyType.Smart) : null;
			var coordinator = new OpenIdCoordinator(
				rp => {
					var request = new CheckIdRequest(protocol.Version, ProviderUri, true);

					if (association != null) {
						rp.AssociationStore.StoreAssociation(ProviderUri, association);
						request.AssociationHandle = association.Handle;
					}

					request.ClaimedIdentifier = "http://claimedid";
					request.LocalIdentifier = "http://localid";
					request.ReturnTo = RPUri;
					rp.Channel.Send(request);
					if (positive) {
						if (tamper) {
							try {
								rp.Channel.ReadFromRequest<PositiveAssertionResponse>();
								Assert.Fail("Expected exception {0} not thrown.", typeof(InvalidSignatureException).Name);
							} catch (InvalidSignatureException) {
								TestLogger.InfoFormat("Caught expected {0} exception after tampering with signed data.", typeof(InvalidSignatureException).Name);
							}
						} else {
							var response = rp.Channel.ReadFromRequest<PositiveAssertionResponse>();
							Assert.IsNotNull(response);
							Assert.AreEqual(request.ClaimedIdentifier, response.ClaimedIdentifier);
							Assert.AreEqual(request.LocalIdentifier, response.LocalIdentifier);
							Assert.AreEqual(request.ReturnTo, response.ReturnTo);

							// Attempt to replay the message and verify that it fails.
							// Because in various scenarios and protocol versions different components
							// notice the replay, we can get one of two exceptions thrown.
							// When the OP notices the replay we get a generic InvalidSignatureException.
							// When the RP notices the replay we get a specific ReplayMessageException.
							Type expectedExceptionType = sharedAssociation || protocol.Version.Major < 2 ? typeof(ReplayedMessageException) : typeof(InvalidSignatureException);
							try {
								CoordinatingChannel channel = (CoordinatingChannel)rp.Channel;
								channel.Replay(response);
								Assert.Fail("Expected exception {0} was not thrown.", expectedExceptionType.Name);
							} catch (ProtocolException ex) {
								Assert.IsInstanceOfType(ex, expectedExceptionType);
							}
						}
					} else {
						var response = rp.Channel.ReadFromRequest<NegativeAssertionResponse>();
						Assert.IsNotNull(response);
						Assert.AreEqual(userSetupUrl, response.UserSetupUrl);
					}
				},
				op => {
					if (association != null) {
						op.AssociationStore.StoreAssociation(AssociationRelyingPartyType.Smart, association);
					}

					var request = op.Channel.ReadFromRequest<CheckIdRequest>();
					Assert.IsNotNull(request);
					IProtocolMessage response;
					if (positive) {
						response = new PositiveAssertionResponse(request);
					} else {
						response = new NegativeAssertionResponse(request) { UserSetupUrl = userSetupUrl };
					}
					op.Channel.Send(response);

					if (positive && !sharedAssociation) {
						var checkauthRequest = op.Channel.ReadFromRequest<CheckAuthenticationRequest>();
						var checkauthResponse = new CheckAuthenticationResponse(checkauthRequest);
						checkauthResponse.IsValid = checkauthRequest.IsValid;
						op.Channel.Send(checkauthResponse);

						if (!tamper) {
							// Respond to the replay attack.
							checkauthRequest = op.Channel.ReadFromRequest<CheckAuthenticationRequest>();
							checkauthResponse = new CheckAuthenticationResponse(checkauthRequest);
							checkauthResponse.IsValid = checkauthRequest.IsValid;
							op.Channel.Send(checkauthResponse);
						}
					}
				});
			if (tamper) {
				coordinator.IncomingMessageFilter = message => {
					var assertion = message as PositiveAssertionResponse;
					if (assertion != null) {
						// Alter the Local Identifier between the Provider and the Relying Party.
						// If the signature binding element does its job, this should cause the RP
						// to throw.
						assertion.LocalIdentifier = "http://victim";
					}
				};
			}
			coordinator.Run();
		}
	}
}
