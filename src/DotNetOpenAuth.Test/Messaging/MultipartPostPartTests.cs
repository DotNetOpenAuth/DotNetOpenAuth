//-----------------------------------------------------------------------
// <copyright file="MultipartPostPartTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System.CodeDom.Compiler;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using NUnit.Framework;

	[TestFixture]
	public class MultipartPostPartTests : TestBase {
		/// <summary>
		/// Verifies that the Length property matches the length actually serialized.
		/// </summary>
		[Test]
		public void FormDataSerializeMatchesLength() {
			var part = MultipartPostPart.CreateFormPart("a", "b");
			VerifyLength(part);
		}

		/// <summary>
		/// Verifies that the length property matches the length actually serialized.
		/// </summary>
		[Test]
		public void FileSerializeMatchesLength() {
			using (TempFileCollection tfc = new TempFileCollection()) {
				string file = tfc.AddExtension(".txt");
				File.WriteAllText(file, "sometext");
				var part = MultipartPostPart.CreateFormFilePart("someformname", file, "text/plain");
				VerifyLength(part);
			}
		}

		/// <summary>
		/// Verifies file multiparts identify themselves as files and not merely form-data.
		/// </summary>
		[Test]
		public void FilePartAsFile() {
			var part = MultipartPostPart.CreateFormFilePart("somename", "somefile", "plain/text", new MemoryStream());
			Assert.AreEqual("file", part.ContentDisposition);
		}

		/// <summary>
		/// Verifies MultiPartPost sends the right number of bytes.
		/// </summary>
		[Test]
		public void MultiPartPostAscii() {
			using (TempFileCollection tfc = new TempFileCollection()) {
				string file = tfc.AddExtension("txt");
				File.WriteAllText(file, "sometext");
				this.VerifyFullPost(new List<MultipartPostPart> {
					MultipartPostPart.CreateFormPart("a", "b"),
					MultipartPostPart.CreateFormFilePart("SomeFormField", file, "text/plain"),
				});
			}
		}

		/// <summary>
		/// Verifies MultiPartPost sends the right number of bytes.
		/// </summary>
		[Test]
		public void MultiPartPostMultiByteCharacters() {
			using (TempFileCollection tfc = new TempFileCollection()) {
				string file = tfc.AddExtension("txt");
				File.WriteAllText(file, "\x1020\x818");
				this.VerifyFullPost(new List<MultipartPostPart> {
					MultipartPostPart.CreateFormPart("a", "\x987"),
					MultipartPostPart.CreateFormFilePart("SomeFormField", file, "text/plain"),
				});
			}
		}

		private static void VerifyLength(MultipartPostPart part) {
			Contract.Requires(part != null);

			var expectedLength = part.Length;
			var ms = new MemoryStream();
			var sw = new StreamWriter(ms);
			part.Serialize(sw);
			sw.Flush();
			var actualLength = ms.Length;
			Assert.AreEqual(expectedLength, actualLength);
		}

		private void VerifyFullPost(List<MultipartPostPart> parts) {
			var request = (HttpWebRequest)WebRequest.Create("http://localhost");
			var handler = new Mocks.TestWebRequestHandler();
			bool posted = false;
			handler.Callback = req => {
				foreach (string header in req.Headers) {
					TestUtilities.TestLogger.InfoFormat("{0}: {1}", header, req.Headers[header]);
				}
				TestUtilities.TestLogger.InfoFormat(handler.RequestEntityAsString);
				Assert.AreEqual(req.ContentLength, handler.RequestEntityStream.Length);
				posted = true;
				return null;
			};
			request.PostMultipart(handler, parts);
			Assert.IsTrue(posted, "HTTP POST never sent.");
		}
	}
}
