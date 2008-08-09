using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using DotNetOpenId;
using DotNetOpenId.Provider;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Test.Hosting;
using DotNetOpenId.Test.Mocks;
using NUnit.Framework;
using IProviderAssociationStore = DotNetOpenId.IAssociationStore<DotNetOpenId.AssociationRelyingPartyType>;
using ProviderMemoryStore = DotNetOpenId.AssociationMemoryStore<DotNetOpenId.AssociationRelyingPartyType>;
using DotNetOpenId.Test.UI;
using log4net;

[SetUpFixture]
public class TestSupport {
	public static readonly string TestWebDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\src\DotNetOpenId.TestWeb"));
	public const string HostTestPage = "HostTest.aspx";
	const string identityPage = "IdentityEndpoint.aspx";
	const string directedIdentityPage = "DirectedIdentityEndpoint.aspx";
	public const string ProviderPage = "ProviderEndpoint.aspx";
	public const string DirectedProviderEndpoint = "DirectedProviderEndpoint.aspx";
	public const string MobileConsumerPage = "RelyingPartyMobile.aspx";
	public const string ConsumerPage = "RelyingParty.aspx";
	public const string OPDefaultPage = "OPDefault.aspx";
	public static Uri ReturnTo { get { return TestSupport.GetFullUrl(TestSupport.ConsumerPage); } }
	public static Realm Realm { get { return new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri); } }
	public readonly static ILog Logger = LogManager.GetLogger("DotNetOpenId.Test");

	public enum Scenarios {
		// Authentication test scenarios
		AutoApproval,
		AutoApprovalAddFragment,
		ApproveOnSetup,
		AlwaysDeny,

		// Extension test scenarios
		/// <summary>
		/// Provides all required and requested fields.
		/// </summary>
		ExtensionFullCooperation,
		/// <summary>
		/// Provides only those fields marked as required.
		/// </summary>
		ExtensionPartialCooperation,
	}
	internal static UriIdentifier GetOPIdentityUrl(Scenarios scenario) {
		return new UriIdentifier(GetFullUrl("/" + OPDefaultPage, "user", scenario));
	}
	internal static UriIdentifier GetIdentityUrl(Scenarios scenario, ProtocolVersion providerVersion) {
		return new UriIdentifier(GetFullUrl("/" + identityPage, new Dictionary<string, string> {
			{ "user", scenario.ToString() },
			{ "version", providerVersion.ToString() },
		}));
	}
	internal static UriIdentifier GetDirectedIdentityUrl(Scenarios scenario, ProtocolVersion providerVersion) {
		return new UriIdentifier(GetFullUrl("/" + directedIdentityPage, new Dictionary<string, string> {
			{ "user", scenario.ToString() },
			{ "version", providerVersion.ToString() },
		}));
	}
	public static Identifier GetDelegateUrl(Scenarios scenario) {
		return new UriIdentifier(GetFullUrl("/" + scenario));
	}
	internal static MockIdentifier GetMockIdentifier(Scenarios scenario, ProtocolVersion providerVersion) {
		ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(
			GetIdentityUrl(scenario, providerVersion),
			GetDelegateUrl(scenario),
			GetFullUrl("/" + ProviderPage),
			new string[] { Protocol.Lookup(providerVersion).ClaimedIdentifierServiceTypeURI },
			10,
			10
			);

		return new MockIdentifier(GetIdentityUrl(scenario, providerVersion), new ServiceEndpoint[] { se });
	}
	internal static MockIdentifier GetMockOPIdentifier(Scenarios scenario, UriIdentifier expectedClaimedId) {
		Uri opEndpoint = GetFullUrl(DirectedProviderEndpoint, "user", scenario);
		ServiceEndpoint se = ServiceEndpoint.CreateForProviderIdentifier(
			GetOPIdentityUrl(scenario),
			opEndpoint,
			new string[] { Protocol.v20.OPIdentifierServiceTypeURI },
			10,
			10
			);

		// Register the Claimed Identifier that directed identity will choose so that RP
		// discovery on that identifier can be mocked up.
		MockHttpRequest.RegisterMockXrdsResponse(expectedClaimedId, se);

		return new MockIdentifier(GetOPIdentityUrl(scenario), new ServiceEndpoint[] { se });
	}
	public static Uri GetFullUrl(string url) {
		return GetFullUrl(url, null);
	}
	public static Uri GetFullUrl(string url, string key, object value) {
		return GetFullUrl(url, new Dictionary<string, string> {
			{ key, value.ToString() },
		});
	}
	public static Uri GetFullUrl(string url, IDictionary<string, string> args) {
		Uri baseUri = UITestSupport.Host != null ? UITestSupport.Host.BaseUri : new Uri("http://localhost/");
		UriBuilder builder = new UriBuilder(new Uri(baseUri, url));
		UriUtil.AppendQueryArgs(builder, args);
		return builder.Uri;
	}

	/// <summary>
	/// Returns the content of a given embedded resource.
	/// </summary>
	/// <param name="path">The path of the file as it appears within the project,
	/// where the leading / marks the root directory of the project.</param>
	internal static string LoadEmbeddedFile(string path) {
		if (!path.StartsWith("/")) path = "/" + path;
		path = "DotNetOpenId.Test" + path.Replace('/', '.');
		Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
		if (resource == null) throw new ArgumentException();
		using (StreamReader sr = new StreamReader(resource)) {
			return sr.ReadToEnd();
		}
	}

	internal static IRelyingPartyApplicationStore RelyingPartyStore;
	internal static IProviderAssociationStore ProviderStore;
	/// <summary>
	/// Generates a new, stateful <see cref="OpenIdRelyingParty"/> whose direct messages
	/// will be automatically handled by an internal <see cref="OpenIdProvider"/>
	/// that uses the shared <see cref="ProviderStore"/>.
	/// </summary>
	internal static OpenIdRelyingParty CreateRelyingParty(NameValueCollection fields) {
		return CreateRelyingParty(RelyingPartyStore, null, fields);
	}
	internal static OpenIdRelyingParty CreateRelyingParty(IRelyingPartyApplicationStore store, NameValueCollection fields) {
		return CreateRelyingParty(store, null, fields);
	}
	/// <summary>
	/// Generates a new <see cref="OpenIdRelyingParty"/> whose direct messages
	/// will be automatically handled by an internal <see cref="OpenIdProvider"/>
	/// that uses the shared <see cref="ProviderStore"/>.
	/// </summary>
	internal static OpenIdRelyingParty CreateRelyingParty(IRelyingPartyApplicationStore store, Uri requestUrl, NameValueCollection fields) {
		var rp = new OpenIdRelyingParty(store, requestUrl ?? GetFullUrl(ConsumerPage), fields ?? new NameValueCollection());
		if (fields == null || fields.Count == 0) {
			Assert.IsNull(rp.Response);
		}
		rp.DirectMessageChannel = new DirectMessageTestRedirector(ProviderStore);
		return rp;
	}
	internal static DotNetOpenId.RelyingParty.IAuthenticationRequest CreateRelyingPartyRequest(bool stateless, Scenarios scenario, ProtocolVersion version) {
		// Publish RP discovery information
		MockHttpRequest.RegisterMockRPDiscovery();

		var rp = TestSupport.CreateRelyingParty(stateless ? null : RelyingPartyStore, null);
		var rpReq = rp.CreateRequest(TestSupport.GetMockIdentifier(scenario, version), Realm, ReturnTo);

		{
			// Sidetrack: verify URLs and other default properties
			Assert.AreEqual(AuthenticationRequestMode.Setup, rpReq.Mode);
			Assert.AreEqual(Realm, rpReq.Realm);
			Assert.AreEqual(ReturnTo, rpReq.ReturnToUrl);
		}

		return rpReq;
	}
	/// <summary>
	/// Generates a new <see cref="OpenIdRelyingParty"/> ready to process a 
	/// response from an <see cref="OpenIdProvider"/>.
	/// </summary>
	internal static IAuthenticationResponse CreateRelyingPartyResponse(IRelyingPartyApplicationStore store, IResponse providerResponse) {
		if (providerResponse == null) throw new ArgumentNullException("providerResponse");

		var opAuthWebResponse = (Response)providerResponse;
		var opAuthResponse = (EncodableResponse)opAuthWebResponse.EncodableMessage;
		var rp = CreateRelyingParty(store, opAuthResponse.RedirectUrl,
			opAuthResponse.EncodedFields.ToNameValueCollection());
		// Get the response now, before trying the replay attack.  The Response
		// property is lazily-evaluated, so the replay attack can be evaluated first
		// and pass, while this one that SUPPOSED to pass fails, if we don't force it now.
		var response = rp.Response;

		// Side-track to test for replay attack while we're at it.
		// This simulates a network sniffing user who caught the 
		// authenticating query en route to either the user agent or
		// the consumer, and tries the same query to the consumer in an
		// attempt to spoof the identity of the authenticating user.
		try {
			var replayRP = CreateRelyingParty(store, opAuthResponse.RedirectUrl,
				opAuthResponse.EncodedFields.ToNameValueCollection());
			Assert.AreNotEqual(AuthenticationStatus.Authenticated, replayRP.Response.Status, "Replay attack succeeded!");
		} catch (OpenIdException) { // nonce already used
			// another way to pass
		}

		// Return the result of the initial response (not the replay attack one).
		return response;
	}
	/// <summary>
	/// Generates a new <see cref="OpenIdProvider"/> that uses the shared
	/// store in <see cref="ProviderStore"/>.
	/// </summary>
	internal static OpenIdProvider CreateProvider(NameValueCollection fields) {
		Protocol protocol = Protocol.Detect(fields.ToDictionary());
		Uri opEndpoint = GetFullUrl(ProviderPage);
		var provider = new OpenIdProvider(ProviderStore, opEndpoint, opEndpoint, fields);
		return provider;
	}
	internal static OpenIdProvider CreateProviderForRequest(DotNetOpenId.RelyingParty.IAuthenticationRequest request) {
		IResponse relyingPartyAuthenticationRequest = request.RedirectingResponse;
		var rpWebMessageToOP = (Response)relyingPartyAuthenticationRequest;
		var rpMessageToOP = (IndirectMessageRequest)rpWebMessageToOP.EncodableMessage;
		var opEndpoint = (ServiceEndpoint)request.Provider;
		var provider = new OpenIdProvider(ProviderStore, opEndpoint.ProviderEndpoint,
			opEndpoint.ProviderEndpoint, rpMessageToOP.EncodedFields.ToNameValueCollection());
		return provider;
	}
	internal static IResponse CreateProviderResponseToRequest(
		DotNetOpenId.RelyingParty.IAuthenticationRequest request,
		Action<DotNetOpenId.Provider.IAuthenticationRequest> prepareProviderResponse) {

		{
			// Sidetrack: Verify the return_to and realm URLs
			var consumerToProviderQuery = HttpUtility.ParseQueryString(request.RedirectingResponse.ExtractUrl().Query);
			Protocol protocol = Protocol.Detect(consumerToProviderQuery.ToDictionary());
			Assert.IsTrue(consumerToProviderQuery[protocol.openid.return_to].StartsWith(request.ReturnToUrl.AbsoluteUri, StringComparison.Ordinal));
			Assert.AreEqual(request.Realm.ToString(), consumerToProviderQuery[protocol.openid.Realm]);
		}

		var op = TestSupport.CreateProviderForRequest(request);
		var opReq = (DotNetOpenId.Provider.IAuthenticationRequest)op.Request;
		prepareProviderResponse(opReq);
		Assert.IsTrue(opReq.IsResponseReady);
		return opReq.Response;
	}
	internal static IAuthenticationResponse CreateRelyingPartyResponseThroughProvider(
		DotNetOpenId.RelyingParty.IAuthenticationRequest request,
		Action<DotNetOpenId.Provider.IAuthenticationRequest> providerAction) {

		var rpReq = (AuthenticationRequest)request;
		var opResponse = CreateProviderResponseToRequest(rpReq, providerAction);
		// Be careful to use whatever store the original RP was using.
		var rp = CreateRelyingPartyResponse(rpReq.RelyingParty.Store, opResponse);
		Assert.IsNotNull(rp);
		return rp;
	}

	[SetUp]
	public void SetUp() {
		log4net.Config.XmlConfigurator.Configure(Assembly.GetExecutingAssembly().GetManifestResourceStream("DotNetOpenId.Test.Logging.config"));

		ResetStores();
	}

	[TearDown]
	public void TearDown() {
		log4net.LogManager.Shutdown();
	}

	internal static void ResetStores() {
		RelyingPartyStore = new ApplicationMemoryStore();
		ProviderStore = new ProviderMemoryStore();
	}

	internal static void SetAuthenticationFromScenario(Scenarios scenario, DotNetOpenId.Provider.IAuthenticationRequest request) {
		Assert.IsTrue(request.IsReturnUrlDiscoverable);
		switch (scenario) {
			case TestSupport.Scenarios.ExtensionFullCooperation:
			case TestSupport.Scenarios.ExtensionPartialCooperation:
			case TestSupport.Scenarios.AutoApproval:
				// immediately approve
				request.IsAuthenticated = true;
				break;
			case TestSupport.Scenarios.AutoApprovalAddFragment:
				request.SetClaimedIdentifierFragment("frag");
				request.IsAuthenticated = true;
				break;
			case TestSupport.Scenarios.ApproveOnSetup:
				request.IsAuthenticated = !request.Immediate;
				break;
			case TestSupport.Scenarios.AlwaysDeny:
				request.IsAuthenticated = false;
				break;
			default:
				throw new InvalidOperationException("Unrecognized scenario");
		}
	}

	/// <summary>
	/// Uses an RPs stored association to resign an altered message from a Provider,
	/// to simulate a Provider that deliberately sent a bad message in an attempt
	/// to thwart RP security.
	/// </summary>
	internal static void Resign(NameValueCollection nvc, IRelyingPartyApplicationStore store) {
		Debug.Assert(nvc != null);
		Debug.Assert(store != null);
		var dict = Util.NameValueCollectionToDictionary(nvc);
		Protocol protocol = Protocol.Detect(dict);
		Uri providerEndpoint = new Uri(nvc[protocol.openid.op_endpoint]);
		string assoc_handle = nvc[protocol.openid.assoc_handle];
		Association assoc = store.GetAssociation(providerEndpoint, assoc_handle);
		Debug.Assert(assoc != null, "Association not found in RP's store.  Maybe you're communicating with a hosted OP instead of the TestSupport one?");
		IList<string> signed = nvc[protocol.openid.signed].Split(',');
		var subsetDictionary = new Dictionary<string, string>();
		foreach (string signedKey in signed) {
			string keyName = protocol.openid.Prefix + signedKey;
			subsetDictionary.Add(signedKey, dict[keyName]);
		}
		nvc[protocol.openid.sig] = Convert.ToBase64String(assoc.Sign(subsetDictionary, signed));
	}

	public static IAssociationStore<AssociationRelyingPartyType> ProviderStoreContext {
		get {
			return DotNetOpenId.Provider.OpenIdProvider.HttpApplicationStore;
		}
	}
}

static class TestExtensions {
	/// <summary>
	/// Gets a URL that can be requested to send an indirect message.
	/// </summary>
	public static Uri ExtractUrl(this IResponse message) {
		DotNetOpenId.Response response = (DotNetOpenId.Response)message;
		IEncodable encodable = response.EncodableMessage;
		UriBuilder builder = new UriBuilder(encodable.RedirectUrl);
		UriUtil.AppendQueryArgs(builder, encodable.EncodedFields);
		return builder.Uri;
	}

	public static NameValueCollection ToNameValueCollection(this IDictionary<string, string> dictionary) {
		NameValueCollection nvc = new NameValueCollection(dictionary.Count);
		foreach (var pair in dictionary) {
			nvc.Add(pair.Key, pair.Value);
		}
		return nvc;
	}
	public static IDictionary<string, string> ToDictionary(this NameValueCollection nvc) {
		Dictionary<string, string> dict = new Dictionary<string, string>(nvc.Count);
		foreach (string key in nvc) {
			dict[key] = nvc[key];
		}
		return dict;
	}
}
