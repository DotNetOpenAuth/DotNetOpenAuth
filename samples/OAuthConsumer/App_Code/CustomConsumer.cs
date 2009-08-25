//-----------------------------------------------------------------------
// <copyright file="CustomConsumer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using System.Net;

/// <summary>
/// A consumer capable of communicating with a cusom provider.
/// </summary>
public static class CustomConsumer {
	/// <summary>
	/// The description of custom OAuth protocol URIs.
	/// </summary>
	public static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription {
		RequestTokenEndpoint = new MessageReceivingEndpoint("http://127.0.0.1:51746/OAuth.ashx", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
		UserAuthorizationEndpoint = new MessageReceivingEndpoint("http://127.0.0.1:51746/OAuth.ashx", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
		AccessTokenEndpoint = new MessageReceivingEndpoint("http://127.0.0.1:51746/OAuth.ashx", HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest),
		TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
	};

	private static readonly MessageReceivingEndpoint GetNameXmlEndpoint = new MessageReceivingEndpoint("http://127.0.0.1:51746/api/GetName?format=xml", HttpDeliveryMethods.PostRequest);
	private static readonly MessageReceivingEndpoint GetAgeXmlEndpoint = new MessageReceivingEndpoint("http://127.0.0.1:51746/api/GetAge?format=xml", HttpDeliveryMethods.PostRequest);
	private static readonly MessageReceivingEndpoint GetFavoriteSitesXmlEndpoint = new MessageReceivingEndpoint("http://127.0.0.1:51746/api/GetFavoriteSites?format=xml", HttpDeliveryMethods.PostRequest);
	private static readonly MessageReceivingEndpoint GetNameJsonEndpoint = new MessageReceivingEndpoint("http://127.0.0.1:51746/api/GetName?format=json", HttpDeliveryMethods.PostRequest);
	private static readonly MessageReceivingEndpoint GetAgeJsonEndpoint = new MessageReceivingEndpoint("http://127.0.0.1:51746/api/GetAge?format=json", HttpDeliveryMethods.PostRequest);
	private static readonly MessageReceivingEndpoint GetFavoriteSitesJsonEndpoint = new MessageReceivingEndpoint("http://127.0.0.1:51746/api/GetFavoriteSites?format=json", HttpDeliveryMethods.PostRequest);

	public static XDocument GetName(ConsumerBase custom, string accessToken) {
		IncomingWebResponse response = custom.PrepareAuthorizedRequestAndSend(GetNameXmlEndpoint, accessToken);
		return XDocument.Load(XmlReader.Create(response.GetResponseReader()));
	}

	#region alternative

	/// <summary>
	/// GetName was proving unreliable due to a bizarre "The connection was forcibly closed" on the 
	/// above call. Never happens with this one so. Need to figure out why. 
	/// Update: Only seems to happen on first request. Odd.
	/// </summary>
	/// <param name="custom"></param>
	/// <param name="accessToken"></param>
	/// <returns></returns>
	public static XDocument GetNameAlternative(ConsumerBase custom, string accessToken) {
		// create the request
		HttpWebRequest request = custom.PrepareAuthorizedRequest(GetNameXmlEndpoint, accessToken, new Dictionary<string, string>());

		// set the Accept as Xml - this is ignored by the request obeject at the moment - shouldn't be.
		request.Accept = "application/xml";

		// get the response
		WebResponse webResponse = request.GetResponse();

		// get the reponse stream
		Stream responseStream = webResponse.GetResponseStream();

		// read it and get full text in response
		StreamReader reader = new StreamReader(responseStream);
		string result = reader.ReadToEnd();

		// return the Xml document
		return XDocument.Parse(result);

	}
	#endregion

	public static String GetNameAsJson(ConsumerBase custom, string accessToken) {
		IncomingWebResponse response = custom.PrepareAuthorizedRequestAndSend(GetNameJsonEndpoint, accessToken);

		// read it and get full text in response
		return response.GetResponseReader().ReadToEnd();
	}

	public static XDocument GetAge(ConsumerBase custom, string accessToken) {
		IncomingWebResponse response = custom.PrepareAuthorizedRequestAndSend(GetAgeXmlEndpoint, accessToken);
		return XDocument.Load(XmlReader.Create(response.GetResponseReader()));
	}

	public static String GetAgeAsJson(ConsumerBase custom, string accessToken) {
		IncomingWebResponse response = custom.PrepareAuthorizedRequestAndSend(GetAgeJsonEndpoint, accessToken);

		// read it and get full text in response
		return response.GetResponseReader().ReadToEnd();
	}

	public static XDocument GetFavoriteSites(ConsumerBase custom, string accessToken) {
		IncomingWebResponse response = custom.PrepareAuthorizedRequestAndSend(GetFavoriteSitesXmlEndpoint, accessToken);
		return XDocument.Load(XmlReader.Create(response.GetResponseReader()));
	}

	public static String GetFavoriteSitesAsJson(ConsumerBase custom, string accessToken) {
		IncomingWebResponse response = custom.PrepareAuthorizedRequestAndSend(GetFavoriteSitesJsonEndpoint, accessToken);

		// read it and get full text in response
		return response.GetResponseReader().ReadToEnd();
	}
}
