//-----------------------------------------------------------------------
// <copyright file="MessagingUtilitiesTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Test.Mocks;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class MessagingUtilitiesTests : TestBase {
		[Test]
		public void CreateQueryString() {
			var args = new Dictionary<string, string>();
			args.Add("a", "b");
			args.Add("c/d", "e/f");
			Assert.AreEqual("a=b&c%2Fd=e%2Ff", MessagingUtilities.CreateQueryString(args));
		}

		[Test]
		public void CreateQueryStringEmptyCollection() {
			Assert.AreEqual(0, MessagingUtilities.CreateQueryString(new Dictionary<string, string>()).Length);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateQueryStringNullDictionary() {
			MessagingUtilities.CreateQueryString(null);
		}

		[Test]
		public void AppendQueryArgs() {
			UriBuilder uri = new UriBuilder("http://baseline.org/page");
			var args = new Dictionary<string, string>();
			args.Add("a", "b");
			args.Add("c/d", "e/f");
			MessagingUtilities.AppendQueryArgs(uri, args);
			Assert.AreEqual("http://baseline.org/page?a=b&c%2Fd=e%2Ff", uri.Uri.AbsoluteUri);
			args.Clear();
			args.Add("g", "h");
			MessagingUtilities.AppendQueryArgs(uri, args);
			Assert.AreEqual("http://baseline.org/page?a=b&c%2Fd=e%2Ff&g=h", uri.Uri.AbsoluteUri);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AppendQueryArgsNullUriBuilder() {
			MessagingUtilities.AppendQueryArgs(null, new Dictionary<string, string>());
		}

		[Test]
		public void AppendQueryArgsNullDictionary() {
			MessagingUtilities.AppendQueryArgs(new UriBuilder(), null);
		}

		[Test]
		public void ToDictionary() {
			NameValueCollection nvc = new NameValueCollection();
			nvc["a"] = "b";
			nvc["c"] = "d";
			nvc[string.Empty] = "emptykey";
			Dictionary<string, string> actual = MessagingUtilities.ToDictionary(nvc);
			Assert.AreEqual(nvc.Count, actual.Count);
			Assert.AreEqual(nvc["a"], actual["a"]);
			Assert.AreEqual(nvc["c"], actual["c"]);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ToDictionaryWithNullKey() {
			NameValueCollection nvc = new NameValueCollection();
			nvc[null] = "a";
			nvc["b"] = "c";
			nvc.ToDictionary(true);
		}

		[Test]
		public void ToDictionaryWithSkippedNullKey() {
			NameValueCollection nvc = new NameValueCollection();
			nvc[null] = "a";
			nvc["b"] = "c";
			var dictionary = nvc.ToDictionary(false);
			Assert.AreEqual(1, dictionary.Count);
			Assert.AreEqual(nvc["b"], dictionary["b"]);
		}

		[Test]
		public void ToDictionaryNull() {
			Assert.IsNull(MessagingUtilities.ToDictionary(null));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ApplyHeadersToResponseNullAspNetResponse() {
			MessagingUtilities.ApplyHeadersToResponse(new WebHeaderCollection(), (HttpResponseBase)null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ApplyHeadersToResponseNullHeaders() {
			MessagingUtilities.ApplyHeadersToResponse(null, new HttpResponseWrapper(new HttpResponse(new StringWriter())));
		}

		[Test]
		public void ApplyHeadersToResponse() {
			var headers = new WebHeaderCollection();
			headers[HttpResponseHeader.ContentType] = "application/binary";

			var response = new HttpResponseWrapper(new HttpResponse(new StringWriter()));
			MessagingUtilities.ApplyHeadersToResponse(headers, response);

			Assert.AreEqual(headers[HttpResponseHeader.ContentType], response.ContentType);
		}

		/// <summary>
		/// Verifies RFC 3986 compliant URI escaping, as required by the OpenID and OAuth specifications.
		/// </summary>
		/// <remarks>
		/// The tests in this method come from http://wiki.oauth.net/TestCases
		/// </remarks>
		[Test]
		public void EscapeUriDataStringRfc3986Tests() {
			Assert.AreEqual("abcABC123", MessagingUtilities.EscapeUriDataStringRfc3986("abcABC123"));
			Assert.AreEqual("-._~", MessagingUtilities.EscapeUriDataStringRfc3986("-._~"));
			Assert.AreEqual("%25", MessagingUtilities.EscapeUriDataStringRfc3986("%"));
			Assert.AreEqual("%2B", MessagingUtilities.EscapeUriDataStringRfc3986("+"));
			Assert.AreEqual("%26%3D%2A", MessagingUtilities.EscapeUriDataStringRfc3986("&=*"));
			Assert.AreEqual("%0A", MessagingUtilities.EscapeUriDataStringRfc3986("\n"));
			Assert.AreEqual("%20", MessagingUtilities.EscapeUriDataStringRfc3986(" "));
			Assert.AreEqual("%7F", MessagingUtilities.EscapeUriDataStringRfc3986("\u007f"));
			Assert.AreEqual("%C2%80", MessagingUtilities.EscapeUriDataStringRfc3986("\u0080"));
			Assert.AreEqual("%E3%80%81", MessagingUtilities.EscapeUriDataStringRfc3986("\u3001"));
		}

		/// <summary>
		/// Verifies proper behavior of GetHttpVerb
		/// </summary>
		[Test]
		public void GetHttpVerbTest() {
			Assert.AreEqual(HttpMethod.Get, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.GetRequest));
			Assert.AreEqual(HttpMethod.Post, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.PostRequest));
			Assert.AreEqual(HttpMethod.Head, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.HeadRequest));
			Assert.AreEqual(HttpMethod.Delete, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.DeleteRequest));
			Assert.AreEqual(HttpMethod.Put, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.PutRequest));
			Assert.AreEqual(new HttpMethod("PATCH"), MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.PatchRequest));
			Assert.AreEqual(HttpMethod.Options, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.OptionsRequest));

			Assert.AreEqual(HttpMethod.Get, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.AuthorizationHeaderRequest));
			Assert.AreEqual(HttpMethod.Post, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest));
			Assert.AreEqual(HttpMethod.Head, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.HeadRequest | HttpDeliveryMethods.AuthorizationHeaderRequest));
			Assert.AreEqual(HttpMethod.Delete, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.DeleteRequest | HttpDeliveryMethods.AuthorizationHeaderRequest));
			Assert.AreEqual(HttpMethod.Put, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.PutRequest | HttpDeliveryMethods.AuthorizationHeaderRequest));
			Assert.AreEqual(new HttpMethod("PATCH"), MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.PatchRequest | HttpDeliveryMethods.AuthorizationHeaderRequest));
			Assert.AreEqual(HttpMethod.Options, MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.OptionsRequest | HttpDeliveryMethods.AuthorizationHeaderRequest));
		}

		/// <summary>
		/// Verifies proper behavior of GetHttpVerb on invalid input.
		/// </summary>
		[Test, ExpectedException(typeof(ArgumentException))]
		public void GetHttpVerbOutOfRangeTest() {
			MessagingUtilities.GetHttpVerb(HttpDeliveryMethods.PutRequest | HttpDeliveryMethods.PostRequest);
		}

		/// <summary>
		/// Verifies proper behavior of GetHttpDeliveryMethod
		/// </summary>
		[Test]
		public void GetHttpDeliveryMethodTest() {
			Assert.AreEqual(HttpDeliveryMethods.GetRequest, MessagingUtilities.GetHttpDeliveryMethod("GET"));
			Assert.AreEqual(HttpDeliveryMethods.PostRequest, MessagingUtilities.GetHttpDeliveryMethod("POST"));
			Assert.AreEqual(HttpDeliveryMethods.HeadRequest, MessagingUtilities.GetHttpDeliveryMethod("HEAD"));
			Assert.AreEqual(HttpDeliveryMethods.PutRequest, MessagingUtilities.GetHttpDeliveryMethod("PUT"));
			Assert.AreEqual(HttpDeliveryMethods.DeleteRequest, MessagingUtilities.GetHttpDeliveryMethod("DELETE"));
			Assert.AreEqual(HttpDeliveryMethods.PatchRequest, MessagingUtilities.GetHttpDeliveryMethod("PATCH"));
			Assert.AreEqual(HttpDeliveryMethods.OptionsRequest, MessagingUtilities.GetHttpDeliveryMethod("OPTIONS"));
		}

		/// <summary>
		/// Verifies proper behavior of GetHttpDeliveryMethod for an unexpected input
		/// </summary>
		[Test, ExpectedException(typeof(ArgumentException))]
		public void GetHttpDeliveryMethodOutOfRangeTest() {
			MessagingUtilities.GetHttpDeliveryMethod("UNRECOGNIZED");
		}

		[Test]
		public void EncryptDecrypt() {
			const string PlainText = "Hi folks!";
			byte[] key = MessagingUtilities.GetCryptoRandomData(128 / 8);
			var cipher = MessagingUtilities.Encrypt(PlainText, key);

			Console.WriteLine("Encrypted \"{0}\" ({1} length) to {2} encrypted bytes.", PlainText, PlainText.Length, cipher.Length);

			string roundTripped = MessagingUtilities.Decrypt(cipher, key);
			Assert.AreEqual(PlainText, roundTripped);
		}

		[Test]
		public void SerializeAsJsonTest() {
			var message = new TestMessageWithDate() {
				Age = 18,
				Timestamp = DateTime.ParseExact("28/04/2012 08:00", "dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture),
				Name = "Andrew",
			};
			string json = MessagingUtilities.SerializeAsJson(message, this.MessageDescriptions);
			Assert.That(json, Is.EqualTo("{\"ts\":\"2012-04-28T08:00:00Z\",\"age\":18,\"Name\":\"Andrew\"}"));
		}

		[Test]
		public void DeserializeFromJson() {
			var message = new TestMessageWithDate();
			string json = "{\"ts\":\"2012-04-28T08:00:00Z\",\"age\":18,\"Name\":\"Andrew\"}";
			MessagingUtilities.DeserializeFromJson(Encoding.UTF8.GetBytes(json), message, this.MessageDescriptions);
			Assert.That(message.Age, Is.EqualTo(18));
			Assert.That(message.Timestamp, Is.EqualTo(DateTime.ParseExact("28/04/2012 08:00", "dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture)));
			Assert.That(message.Name, Is.EqualTo("Andrew"));
		}

		/// <summary>
		/// Verifies that the time-independent string equality check works accurately.
		/// </summary>
		[Test]
		public void EqualsConstantTime() {
			this.EqualsConstantTimeHelper(null, null);
			this.EqualsConstantTimeHelper(null, string.Empty);
			this.EqualsConstantTimeHelper(string.Empty, string.Empty);
			this.EqualsConstantTimeHelper(string.Empty, "a");
			this.EqualsConstantTimeHelper(null, "a");
			this.EqualsConstantTimeHelper("a", "a");
			this.EqualsConstantTimeHelper("a", "A");
			this.EqualsConstantTimeHelper("A", "A");
			this.EqualsConstantTimeHelper("ab", "ab");
			this.EqualsConstantTimeHelper("ab", "b");
		}

		/// <summary>
		/// Verifies that EqualsConstantTime actually has the same execution time regardless of how well a value matches.
		/// </summary>
		[Test, Category("Performance")]
		public void EqualsConstantTimeIsActuallyConstantTime() {
			string expected = new string('A', 5000);
			string totalmismatch = new string('B', 5000);
			string almostmatch = new string('A', 4999) + 'B';

			const int Iterations = 4000;
			var totalMismatchTimer = new Stopwatch();
			totalMismatchTimer.Start();
			for (int i = 0; i < Iterations; i++) {
				MessagingUtilities.EqualsConstantTime(expected, totalmismatch);
			}
			totalMismatchTimer.Stop();

			var almostMatchTimer = new Stopwatch();
			almostMatchTimer.Start();
			for (int i = 0; i < Iterations; i++) {
				MessagingUtilities.EqualsConstantTime(expected, almostmatch);
			}
			almostMatchTimer.Stop();

			const double ToleranceFactor = 0.12;
			long averageTimeTicks = (totalMismatchTimer.ElapsedTicks + almostMatchTimer.ElapsedTicks) / 2;
			var tolerableDifference = TimeSpan.FromTicks((long)(averageTimeTicks * ToleranceFactor));
			var absoluteDifference = TimeSpan.FromTicks(Math.Abs(totalMismatchTimer.ElapsedTicks - almostMatchTimer.ElapsedTicks));
			double actualFactor = (double)absoluteDifference.Ticks / averageTimeTicks;
			Assert.IsTrue(absoluteDifference <= tolerableDifference, "A total mismatch took {0} but a near match took {1}, which is too different to be indistinguishable.  The tolerable difference is {2} but the actual difference is {3}.  This represents a difference of {4}%, beyond the tolerated {5}%.", totalMismatchTimer.Elapsed, almostMatchTimer.Elapsed, tolerableDifference, absoluteDifference, Math.Round(actualFactor * 100), Math.Round(ToleranceFactor * 100));
			Console.WriteLine("A total mismatch took {0} and a near match took {1}.  The tolerable difference is {2}, and the actual difference is {3}.  This represents a difference of {4}%, within the tolerated {5}%.", totalMismatchTimer.Elapsed, almostMatchTimer.Elapsed, tolerableDifference, absoluteDifference, Math.Round(actualFactor * 100), Math.Round(ToleranceFactor * 100));
			Console.WriteLine("The equality test execution time difference was only {0}%, within the tolerable {1}%", Math.Round(100 * actualFactor), Math.Round(ToleranceFactor * 100));
		}

		/// <summary>
		/// Verifies that the time-independent string equality check works for a given pair of strings.
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		private void EqualsConstantTimeHelper(string value1, string value2) {
			bool expected = string.Equals(value1, value2, StringComparison.Ordinal);
			Assert.AreEqual(expected, MessagingUtilities.EqualsConstantTime(value1, value2));
			Assert.AreEqual(expected, MessagingUtilities.EqualsConstantTime(value2, value1));
		}
	}
}
