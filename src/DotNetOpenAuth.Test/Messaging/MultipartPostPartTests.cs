//-----------------------------------------------------------------------
// <copyright file="MultipartPostPartTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System.CodeDom.Compiler;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class MultipartPostPartTests : TestBase {
		/// <summary>
		/// Verifies that the Length property matches the length actually serialized.
		/// </summary>
		[TestMethod]
		public void FormDataSerializeMatchesLength() {
			var part = MultipartPostPart.CreateFormPart("a", "b");
			VerifyLength(part);
		}

		/// <summary>
		/// Verifies that the length property matches the length actually serialized.
		/// </summary>
		[TestMethod]
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
		[TestMethod]
		public void FilePartAsFile() {
			var part = MultipartPostPart.CreateFormFilePart("somename", "somefile", "plain/text", new MemoryStream());
			Assert.AreEqual("file", part.ContentDisposition);
		}

		/// <summary>
		/// Verifies MultiPartPost sends the right number of bytes.
		/// </summary>
		[TestMethod]
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
		[TestMethod]
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
					TestContext.WriteLine("{0}: {1}", header, req.Headers[header]);
				}
				TestContext.WriteLine(handler.RequestEntityAsString);
				Assert.AreEqual(req.ContentLength, handler.RequestEntityStream.Length);
				posted = true;
				return null;
			};
			request.PostMultipart(handler, parts);
			Assert.IsTrue(posted, "HTTP POST never sent.");
		}
	}
}
