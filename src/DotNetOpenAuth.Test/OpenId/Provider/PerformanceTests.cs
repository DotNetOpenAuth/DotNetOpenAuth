//-----------------------------------------------------------------------
// <copyright file="PerformanceTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class PerformanceTests : OpenIdTestBase {
		private const string SharedAssociationHandle = "handle";
		private static readonly TimeSpan TestRunTime = TimeSpan.FromSeconds(3);
		private OpenIdProvider provider;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
			SuspendLogging();
			this.provider = CreateProvider();
		}

		[TestCleanup]
		public override void Cleanup() {
			ResumeLogging();
			base.Cleanup();
		}

		[TestMethod]
		public void AssociateDH() {
			var associateRequest = this.CreateAssociateRequest(OPUri);
			Stopwatch timer = new Stopwatch();
			timer.Start();
			int iterations;
			for (iterations = 0; timer.ElapsedMilliseconds < TestRunTime.TotalMilliseconds; iterations++) {
				IRequest request = this.provider.GetRequest(associateRequest);
				var response = this.provider.PrepareResponse(request);
				Assert.IsInstanceOfType(response.OriginalMessage, typeof(AssociateSuccessfulResponse));
			}
			timer.Stop();
			double executionsPerSecond = GetExecutionsPerSecond(iterations, timer);
			TestContext.WriteLine("Created {0} associations in {1}, or {2} per second.", iterations, timer.Elapsed, executionsPerSecond);
			Assert.IsTrue(executionsPerSecond >= 2, "Too slow");
		}

		[TestMethod]
		public void AssociateClearText() {
			var associateRequest = this.CreateAssociateRequest(OPUriSsl); // SSL will cause a plaintext association
			Stopwatch timer = new Stopwatch();
			timer.Start();
			int iterations;
			for (iterations = 0; timer.ElapsedMilliseconds < TestRunTime.TotalMilliseconds; iterations++) {
				IRequest request = this.provider.GetRequest(associateRequest);
				var response = this.provider.PrepareResponse(request);
				Assert.IsInstanceOfType(response.OriginalMessage, typeof(AssociateSuccessfulResponse));
			}
			timer.Stop();
			double executionsPerSecond = GetExecutionsPerSecond(iterations, timer);
			TestContext.WriteLine("Created {0} associations in {1}, or {2} per second.", iterations, timer.Elapsed, executionsPerSecond);
			Assert.IsTrue(executionsPerSecond > 1000, "Too slow.");
		}

		[TestMethod]
		public void CheckIdSharedHmacSha1Association() {
			Protocol protocol = Protocol.Default;
			string assocType = protocol.Args.SignatureAlgorithm.HMAC_SHA1;
			double executionsPerSecond = this.ParameterizedCheckIdTest(protocol, assocType);
			Assert.IsTrue(executionsPerSecond > 500, "Too slow");
		}

		[TestMethod]
		public void CheckIdSharedHmacSha256Association() {
			Protocol protocol = Protocol.Default;
			string assocType = protocol.Args.SignatureAlgorithm.HMAC_SHA256;
			double executionsPerSecond = this.ParameterizedCheckIdTest(protocol, assocType);
			Assert.IsTrue(executionsPerSecond > 400, "Too slow");
		}

		private static double GetExecutionsPerSecond(int iterations, Stopwatch timer) {
			return (double)iterations / (timer.ElapsedMilliseconds / 1000);
		}

		private double ParameterizedCheckIdTest(Protocol protocol, string assocType) {
			Association assoc = HmacShaAssociation.Create(
				protocol,
				assocType,
				AssociationRelyingPartyType.Smart,
				this.provider.SecuritySettings);
			this.provider.AssociationStore.StoreAssociation(AssociationRelyingPartyType.Smart, assoc);
			var checkidRequest = this.CreateCheckIdRequest(true);
			Stopwatch timer = new Stopwatch();
			timer.Start();
			int iterations;
			for (iterations = 0; timer.ElapsedMilliseconds < TestRunTime.TotalMilliseconds; iterations++) {
				var request = (IAuthenticationRequest)this.provider.GetRequest(checkidRequest);
				request.IsAuthenticated = true;
				var response = this.provider.PrepareResponse(request);
				Assert.IsInstanceOfType(response.OriginalMessage, typeof(PositiveAssertionResponse));
			}
			timer.Stop();
			double executionsPerSecond = GetExecutionsPerSecond(iterations, timer);
			TestContext.WriteLine("Responded to {0} checkid messages in {1}; or {2} authentications per second.", iterations, timer.Elapsed, executionsPerSecond);
			return executionsPerSecond;
		}

		private HttpRequestInfo CreateAssociateRequest(Uri opEndpoint) {
			var rp = CreateRelyingParty(true);
			AssociateRequest associateMessage = AssociateRequest.Create(rp.SecuritySettings, new ProviderEndpointDescription(opEndpoint, Protocol.Default.Version));
			Channel rpChannel = rp.Channel;
			MemoryStream ms = new MemoryStream();
			StreamWriter mswriter = new StreamWriter(ms);
			mswriter.Write(MessagingUtilities.CreateQueryString(rpChannel.MessageDescriptions.GetAccessor(associateMessage)));
			mswriter.Flush();
			ms.Position = 0;
			var headers = new WebHeaderCollection();
			headers.Add(HttpRequestHeader.ContentType, Channel.HttpFormUrlEncoded);
			var httpRequest = new HttpRequestInfo("POST", opEndpoint, opEndpoint.PathAndQuery, headers, ms);
			return httpRequest;
		}

		private HttpRequestInfo CreateCheckIdRequest(bool sharedAssociation) {
			var rp = CreateRelyingParty(true);
			CheckIdRequest checkidMessage = new CheckIdRequest(
				Protocol.Default.Version,
				OPUri,
				DotNetOpenAuth.OpenId.RelyingParty.AuthenticationRequestMode.Setup);
			if (sharedAssociation) {
				checkidMessage.AssociationHandle = SharedAssociationHandle;
			}
			checkidMessage.ClaimedIdentifier = OPLocalIdentifiers[0];
			checkidMessage.LocalIdentifier = OPLocalIdentifiers[0];
			checkidMessage.Realm = RPRealmUri;
			checkidMessage.ReturnTo = RPUri;
			Channel rpChannel = rp.Channel;
			UriBuilder receiver = new UriBuilder(OPUri);
			receiver.Query = MessagingUtilities.CreateQueryString(rpChannel.MessageDescriptions.GetAccessor(checkidMessage));
			var headers = new WebHeaderCollection();
			var httpRequest = new HttpRequestInfo("GET", receiver.Uri, receiver.Uri.PathAndQuery, headers, null);
			return httpRequest;
		}
	}
}
