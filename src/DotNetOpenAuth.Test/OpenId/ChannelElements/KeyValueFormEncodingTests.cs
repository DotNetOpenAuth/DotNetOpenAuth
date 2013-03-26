//-----------------------------------------------------------------------
// <copyright file="KeyValueFormEncodingTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using NUnit.Framework;

	[TestFixture]
	public class KeyValueFormEncodingTests : TestBase {
		private Dictionary<string, string> sampleData = new Dictionary<string, string> {
			{ "key1", "value1" },
			{ "key2", "value2:a" },
			{ "Key3", "Value3" },
		};

		private KeyValueFormEncoding keyValueForm = new KeyValueFormEncoding();

		[Flags]
		public enum TestMode {
			Encoder = 0x1,
			Decoder = 0x2,
			Both = 0x3,
		}

		[Test]
		public void BasicEncodingTest() {
			byte[] kvfBytes = KeyValueFormEncoding.GetBytes(this.sampleData);
			string responseString = Encoding.UTF8.GetString(kvfBytes);

			Assert.IsFalse(responseString.Contains("\n\n"));
			Assert.IsTrue(responseString.EndsWith("\n", StringComparison.Ordinal));
			int count = 0;
			foreach (string line in responseString.Split('\n')) {
				if (line.Length == 0) {
					break;
				}
				int colon = line.IndexOf(':');
				Assert.IsTrue(colon > 0);
				string key = line.Substring(0, colon);
				string value = line.Substring(colon + 1);
				Assert.AreEqual(this.sampleData[key], value);
				count++;
			}

			Assert.AreEqual(this.sampleData.Count, count);
		}

		public async Task KVDictTestAsync(byte[] kvform, IDictionary<string, string> dict, TestMode mode) {
			if ((mode & TestMode.Decoder) == TestMode.Decoder) {
				var d = await this.keyValueForm.GetDictionaryAsync(new MemoryStream(kvform), CancellationToken.None);
				foreach (string key in dict.Keys) {
					Assert.AreEqual(d[key], dict[key], "Decoder fault: " + d[key] + " and " + dict[key] + " do not match.");
				}
			}
			if ((mode & TestMode.Encoder) == TestMode.Encoder) {
				var e = KeyValueFormEncoding.GetBytes(dict);
				Assert.IsTrue(MessagingUtilities.AreEquivalent(e, kvform), "Encoder did not produced expected result.");
			}
		}

		[Test]
		public async Task EncodeDecode() {
			await this.KVDictTestAsync(UTF8Encoding.UTF8.GetBytes(string.Empty), new Dictionary<string, string>(), TestMode.Both);

			Dictionary<string, string> d1 = new Dictionary<string, string>();
			d1.Add("college", "harvey mudd");
			await this.KVDictTestAsync(UTF8Encoding.UTF8.GetBytes("college:harvey mudd\n"), d1, TestMode.Both);

			Dictionary<string, string> d2 = new Dictionary<string, string>();
			d2.Add("city", "claremont");
			d2.Add("state", "CA");
			await this.KVDictTestAsync(UTF8Encoding.UTF8.GetBytes("city:claremont\nstate:CA\n"), d2, TestMode.Both);

			Dictionary<string, string> d3 = new Dictionary<string, string>();
			d3.Add("is_valid", "true");
			d3.Add("invalidate_handle", "{HMAC-SHA1:2398410938412093}");
			await this.KVDictTestAsync(UTF8Encoding.UTF8.GetBytes("is_valid:true\ninvalidate_handle:{HMAC-SHA1:2398410938412093}\n"), d3, TestMode.Both);

			Dictionary<string, string> d4 = new Dictionary<string, string>();
			d4.Add(string.Empty, string.Empty);
			await this.KVDictTestAsync(UTF8Encoding.UTF8.GetBytes(":\n"), d4, TestMode.Both);

			Dictionary<string, string> d5 = new Dictionary<string, string>();
			d5.Add(string.Empty, "missingkey");
			await this.KVDictTestAsync(UTF8Encoding.UTF8.GetBytes(":missingkey\n"), d5, TestMode.Both);

			Dictionary<string, string> d6 = new Dictionary<string, string>();
			d6.Add("street", "foothill blvd");
			await this.KVDictTestAsync(UTF8Encoding.UTF8.GetBytes("street:foothill blvd\n"), d6, TestMode.Both);

			Dictionary<string, string> d7 = new Dictionary<string, string>();
			d7.Add("major", "computer science");
			await this.KVDictTestAsync(UTF8Encoding.UTF8.GetBytes("major:computer science\n"), d7, TestMode.Both);

			Dictionary<string, string> d8 = new Dictionary<string, string>();
			d8.Add("dorm", "east");
			await this.KVDictTestAsync(UTF8Encoding.UTF8.GetBytes(" dorm : east \n"), d8, TestMode.Decoder);

			Dictionary<string, string> d9 = new Dictionary<string, string>();
			d9.Add("e^(i*pi)+1", "0");
			await this.KVDictTestAsync(UTF8Encoding.UTF8.GetBytes("e^(i*pi)+1:0"), d9, TestMode.Decoder);

			Dictionary<string, string> d10 = new Dictionary<string, string>();
			d10.Add("east", "west");
			d10.Add("north", "south");
			await this.KVDictTestAsync(UTF8Encoding.UTF8.GetBytes("east:west\nnorth:south"), d10, TestMode.Decoder);
		}

		[Test, ExpectedException(typeof(FormatException))]
		public async Task NoValue() {
			await this.IllegalAsync("x\n", KeyValueFormConformanceLevel.OpenId11);
		}

		[Test, ExpectedException(typeof(FormatException))]
		public async Task NoValueLoose() {
			Dictionary<string, string> d = new Dictionary<string, string>();
			await this.KVDictTestAsync(Encoding.UTF8.GetBytes("x\n"), d, TestMode.Decoder);
		}

		[Test, ExpectedException(typeof(FormatException))]
		public async Task EmptyLine() {
			await this.IllegalAsync("x:b\n\n", KeyValueFormConformanceLevel.OpenId20);
		}

		[Test]
		public async Task EmptyLineLoose() {
			Dictionary<string, string> d = new Dictionary<string, string>();
			d.Add("x", "b");
			await this.KVDictTestAsync(Encoding.UTF8.GetBytes("x:b\n\n"), d, TestMode.Decoder);
		}

		[Test, ExpectedException(typeof(FormatException))]
		public async Task LastLineNotTerminated() {
			await this.IllegalAsync("x:y\na:b", KeyValueFormConformanceLevel.OpenId11);
		}

		[Test]
		public async Task LastLineNotTerminatedLoose() {
			Dictionary<string, string> d = new Dictionary<string, string>();
			d.Add("x", "y");
			d.Add("a", "b");
			await this.KVDictTestAsync(Encoding.UTF8.GetBytes("x:y\na:b"), d, TestMode.Decoder);
		}

		private async Task IllegalAsync(string s, KeyValueFormConformanceLevel level) {
			await new KeyValueFormEncoding(level).GetDictionaryAsync(new MemoryStream(Encoding.UTF8.GetBytes(s)), CancellationToken.None);
		}
	}
}
