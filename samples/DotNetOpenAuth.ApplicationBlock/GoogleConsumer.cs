//-----------------------------------------------------------------------
// <copyright file="GoogleConsumer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml;
	using System.Xml.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// A consumer capable of communicating with Google Data APIs.
	/// </summary>
	public class GoogleConsumer : Consumer {
		/// <summary>
		/// The Consumer to use for accessing Google data APIs.
		/// </summary>
		public static readonly ServiceProviderDescription ServiceDescription =
			new ServiceProviderDescription(
				"https://www.google.com/accounts/OAuthGetRequestToken",
				"https://www.google.com/accounts/OAuthAuthorizeToken",
				"https://www.google.com/accounts/OAuthGetAccessToken");

		/// <summary>
		/// A mapping between Google's applications and their URI scope values.
		/// </summary>
		private static readonly Dictionary<Applications, string> DataScopeUris = new Dictionary<Applications, string> {
			{ Applications.Analytics, "https://www.google.com/analytics/feeds/" },
			{ Applications.GoogleBase, "http://www.google.com/base/feeds/" },
			{ Applications.Blogger, "http://www.blogger.com/feeds" },
			{ Applications.BookSearch, "http://www.google.com/books/feeds/" },
			{ Applications.Calendar, "http://www.google.com/calendar/feeds/" },
			{ Applications.Contacts, "http://www.google.com/m8/feeds/" },
			{ Applications.DocumentsList, "http://docs.google.com/feeds/" },
			{ Applications.Finance, "http://finance.google.com/finance/feeds/" },
			{ Applications.Gmail, "https://mail.google.com/mail/feed/atom" },
			{ Applications.Health, "https://www.google.com/h9/feeds/" },
			{ Applications.Maps, "http://maps.google.com/maps/feeds/" },
			{ Applications.OpenSocial, "http://sandbox.gmodules.com/api/" },
			{ Applications.PicasaWeb, "http://picasaweb.google.com/data/" },
			{ Applications.Spreadsheets, "http://spreadsheets.google.com/feeds/" },
			{ Applications.WebmasterTools, "http://www.google.com/webmasters/tools/feeds/" },
			{ Applications.YouTube, "http://gdata.youtube.com" },
		};

		/// <summary>
		/// The URI to get contacts once authorization is granted.
		/// </summary>
		private static readonly Uri GetContactsEndpoint = new Uri("http://www.google.com/m8/feeds/contacts/default/full/");

		/// <summary>
		/// Initializes a new instance of the <see cref="GoogleConsumer"/> class.
		/// </summary>
		public GoogleConsumer() {
			this.ServiceProvider = ServiceDescription;
			this.ConsumerKey = ConfigurationManager.AppSettings["googleConsumerKey"];
			this.ConsumerSecret = ConfigurationManager.AppSettings["googleConsumerSecret"];
			this.TemporaryCredentialStorage = HttpContext.Current != null
												  ? (ITemporaryCredentialStorage)new CookieTemporaryCredentialStorage()
												  : new MemoryTemporaryCredentialStorage();
		}

		/// <summary>
		/// The many specific authorization scopes Google offers.
		/// </summary>
		[Flags]
		public enum Applications : long {
			/// <summary>
			/// The Gmail address book.
			/// </summary>
			Contacts = 0x1,

			/// <summary>
			/// Appointments in Google Calendar.
			/// </summary>
			Calendar = 0x2,

			/// <summary>
			/// Blog post authoring.
			/// </summary>
			Blogger = 0x4,

			/// <summary>
			/// Google Finance
			/// </summary>
			Finance = 0x8,

			/// <summary>
			/// Google Gmail
			/// </summary>
			Gmail = 0x10,

			/// <summary>
			/// Google Health
			/// </summary>
			Health = 0x20,

			/// <summary>
			/// Google OpenSocial
			/// </summary>
			OpenSocial = 0x40,

			/// <summary>
			/// Picasa Web
			/// </summary>
			PicasaWeb = 0x80,

			/// <summary>
			/// Google Spreadsheets
			/// </summary>
			Spreadsheets = 0x100,

			/// <summary>
			/// Webmaster Tools
			/// </summary>
			WebmasterTools = 0x200,

			/// <summary>
			/// YouTube service
			/// </summary>
			YouTube = 0x400,

			/// <summary>
			/// Google Docs
			/// </summary>
			DocumentsList = 0x800,

			/// <summary>
			/// Google Book Search
			/// </summary>
			BookSearch = 0x1000,

			/// <summary>
			/// Google Base
			/// </summary>
			GoogleBase = 0x2000,

			/// <summary>
			/// Google Analytics
			/// </summary>
			Analytics = 0x4000,

			/// <summary>
			/// Google Maps
			/// </summary>
			Maps = 0x8000,
		}

		/// <summary>
		/// Gets the scope URI in Google's format.
		/// </summary>
		/// <param name="scope">The scope, which may include one or several Google applications.</param>
		/// <returns>A space-delimited list of URIs for the requested Google applications.</returns>
		public static string GetScopeUri(Applications scope) {
			return string.Join(" ", Util.GetIndividualFlags(scope).Select(app => DataScopeUris[(Applications)app]).ToArray());
		}

		/// <summary>
		/// Requests authorization from Google to access data from a set of Google applications.
		/// </summary>
		/// <param name="requestedAccessScope">The requested access scope.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		public Task<Uri> RequestUserAuthorizationAsync(Applications requestedAccessScope, CancellationToken cancellationToken = default(CancellationToken)) {
			var extraParameters = new Dictionary<string, string> {
				{ "scope", GetScopeUri(requestedAccessScope) },
			};
			Uri callback = Util.GetCallbackUrlFromContext();
			return this.RequestUserAuthorizationAsync(callback, extraParameters, cancellationToken);
		}

		/// <summary>
		/// Gets the Gmail address book's contents.
		/// </summary>
		/// <param name="accessToken">The access token previously retrieved.</param>
		/// <param name="maxResults">The maximum number of entries to return. If you want to receive all of the contacts, rather than only the default maximum, you can specify a very large number here.</param>
		/// <param name="startIndex">The 1-based index of the first result to be retrieved (for paging).</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An XML document returned by Google.
		/// </returns>
		public async Task<XDocument> GetContactsAsync(AccessToken accessToken, int maxResults = 25, int startIndex = 1, CancellationToken cancellationToken = default(CancellationToken)) {
			// Enable gzip compression.  Google only compresses the response for recognized user agent headers. - Mike Lim
			var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip };
			using (var httpClient = this.CreateHttpClient(accessToken, handler)) {
				var request = new HttpRequestMessage(HttpMethod.Get, GetContactsEndpoint);
				request.Content = new FormUrlEncodedContent(
					new Dictionary<string, string>() {
						{ "start-index", startIndex.ToString(CultureInfo.InvariantCulture) },
						{ "max-results", maxResults.ToString(CultureInfo.InvariantCulture) },
					});
				request.Headers.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.16 (KHTML, like Gecko) Chrome/10.0.648.151 Safari/534.16"));
				using (var response = await httpClient.SendAsync(request, cancellationToken)) {
					string body = await response.Content.ReadAsStringAsync();
					XDocument result = XDocument.Parse(body);
					return result;
				}
			}
		}

		public async Task PostBlogEntryAsync(AccessToken accessToken, string blogUrl, string title, XElement body, CancellationToken cancellationToken = default(CancellationToken)) {
			string feedUrl;
			var getBlogHome = WebRequest.Create(blogUrl);
			using (var blogHomeResponse = getBlogHome.GetResponse()) {
				using (StreamReader sr = new StreamReader(blogHomeResponse.GetResponseStream())) {
					string homePageHtml = sr.ReadToEnd();
					Match m = Regex.Match(homePageHtml, @"http://www.blogger.com/feeds/\d+/posts/default");
					Debug.Assert(m.Success, "Posting operation failed.");
					feedUrl = m.Value;
				}
			}
			const string Atom = "http://www.w3.org/2005/Atom";
			XElement entry = new XElement(
				XName.Get("entry", Atom),
				new XElement(XName.Get("title", Atom), new XAttribute("type", "text"), title),
				new XElement(XName.Get("content", Atom), new XAttribute("type", "xhtml"), body),
				new XElement(XName.Get("category", Atom), new XAttribute("scheme", "http://www.blogger.com/atom/ns#"), new XAttribute("term", "oauthdemo")));

			MemoryStream ms = new MemoryStream();
			XmlWriterSettings xws = new XmlWriterSettings() {
				Encoding = Encoding.UTF8,
			};
			XmlWriter xw = XmlWriter.Create(ms, xws);
			entry.WriteTo(xw);
			xw.Flush();
			ms.Seek(0, SeekOrigin.Begin);

			var request = new HttpRequestMessage(HttpMethod.Post, feedUrl);
			request.Content = new StreamContent(ms);
			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/atom+xml");
			using (var httpClient = this.CreateHttpClient(accessToken)) {
				using (var response = await httpClient.SendAsync(request, cancellationToken)) {
					if (response.StatusCode == HttpStatusCode.Created) {
						// Success
					} else {
						// Error!
						response.EnsureSuccessStatusCode(); // throw some meaningful exception.
					}
				}
			}
		}
	}
}
