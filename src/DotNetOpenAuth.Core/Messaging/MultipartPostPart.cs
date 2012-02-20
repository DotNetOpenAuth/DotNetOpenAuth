//-----------------------------------------------------------------------
// <copyright file="MultipartPostPart.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Net;
	using System.Text;

	/// <summary>
	/// Represents a single part in a HTTP multipart POST request.
	/// </summary>
	public class MultipartPostPart : IDisposable {
		/// <summary>
		/// The "Content-Disposition" string.
		/// </summary>
		private const string ContentDispositionHeader = "Content-Disposition";

		/// <summary>
		/// The two-character \r\n newline character sequence to use.
		/// </summary>
		private const string NewLine = "\r\n";

		/// <summary>
		/// Initializes a new instance of the <see cref="MultipartPostPart"/> class.
		/// </summary>
		/// <param name="contentDisposition">The content disposition of the part.</param>
		public MultipartPostPart(string contentDisposition) {
			Requires.NotNullOrEmpty(contentDisposition, "contentDisposition");

			this.ContentDisposition = contentDisposition;
			this.ContentAttributes = new Dictionary<string, string>();
			this.PartHeaders = new WebHeaderCollection();
		}

		/// <summary>
		/// Gets or sets the content disposition.
		/// </summary>
		/// <value>The content disposition.</value>
		public string ContentDisposition { get; set; }

		/// <summary>
		/// Gets the key=value attributes that appear on the same line as the Content-Disposition.
		/// </summary>
		/// <value>The content attributes.</value>
		public IDictionary<string, string> ContentAttributes { get; private set; }

		/// <summary>
		/// Gets the headers that appear on subsequent lines after the Content-Disposition.
		/// </summary>
		public WebHeaderCollection PartHeaders { get; private set; }

		/// <summary>
		/// Gets or sets the content of the part.
		/// </summary>
		public Stream Content { get; set; }

		/// <summary>
		/// Gets the length of this entire part.
		/// </summary>
		/// <remarks>Useful for calculating the ContentLength HTTP header to send before actually serializing the content.</remarks>
		public long Length {
			get {
				ErrorUtilities.VerifyOperation(this.Content != null && this.Content.Length >= 0, MessagingStrings.StreamMustHaveKnownLength);

				long length = 0;
				length += ContentDispositionHeader.Length;
				length += ": ".Length;
				length += this.ContentDisposition.Length;
				foreach (var pair in this.ContentAttributes) {
					length += "; ".Length + pair.Key.Length + "=\"".Length + pair.Value.Length + "\"".Length;
				}

				length += NewLine.Length;
				foreach (string headerName in this.PartHeaders) {
					length += headerName.Length;
					length += ": ".Length;
					length += this.PartHeaders[headerName].Length;
					length += NewLine.Length;
				}

				length += NewLine.Length;
				length += this.Content.Length;

				return length;
			}
		}

		/// <summary>
		/// Creates a part that represents a simple form field.
		/// </summary>
		/// <param name="name">The name of the form field.</param>
		/// <param name="value">The value.</param>
		/// <returns>The constructed part.</returns>
		public static MultipartPostPart CreateFormPart(string name, string value) {
			Requires.NotNullOrEmpty(name, "name");
			Requires.NotNull(value, "value");

			var part = new MultipartPostPart("form-data");
			try {
				part.ContentAttributes["name"] = name;
				part.Content = new MemoryStream(Encoding.UTF8.GetBytes(value));
				return part;
			} catch {
				part.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Creates a part that represents a file attachment.
		/// </summary>
		/// <param name="name">The name of the form field.</param>
		/// <param name="filePath">The path to the file to send.</param>
		/// <param name="contentType">Type of the content in HTTP Content-Type format.</param>
		/// <returns>The constructed part.</returns>
		public static MultipartPostPart CreateFormFilePart(string name, string filePath, string contentType) {
			Requires.NotNullOrEmpty(name, "name");
			Requires.NotNullOrEmpty(filePath, "filePath");
			Requires.NotNullOrEmpty(contentType, "contentType");

			string fileName = Path.GetFileName(filePath);
			var fileStream = File.OpenRead(filePath);
			try {
				return CreateFormFilePart(name, fileName, contentType, fileStream);
			} catch {
				fileStream.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Creates a part that represents a file attachment.
		/// </summary>
		/// <param name="name">The name of the form field.</param>
		/// <param name="fileName">Name of the file as the server should see it.</param>
		/// <param name="contentType">Type of the content in HTTP Content-Type format.</param>
		/// <param name="content">The content of the file.</param>
		/// <returns>The constructed part.</returns>
		public static MultipartPostPart CreateFormFilePart(string name, string fileName, string contentType, Stream content) {
			Requires.NotNullOrEmpty(name, "name");
			Requires.NotNullOrEmpty(fileName, "fileName");
			Requires.NotNullOrEmpty(contentType, "contentType");
			Requires.NotNull(content, "content");

			var part = new MultipartPostPart("file");
			try {
				part.ContentAttributes["name"] = name;
				part.ContentAttributes["filename"] = fileName;
				part.PartHeaders[HttpRequestHeader.ContentType] = contentType;
				if (!contentType.StartsWith("text/", StringComparison.Ordinal)) {
					part.PartHeaders["Content-Transfer-Encoding"] = "binary";
				}

				part.Content = content;
				return part;
			} catch {
				part.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Serializes the part to a stream.
		/// </summary>
		/// <param name="streamWriter">The stream writer.</param>
		internal void Serialize(StreamWriter streamWriter) {
			// VERY IMPORTANT: any changes at all made to this must be kept in sync with the
			// Length property which calculates exactly how many bytes this method will write.
			streamWriter.NewLine = NewLine;
			streamWriter.Write("{0}: {1}", ContentDispositionHeader, this.ContentDisposition);
			foreach (var pair in this.ContentAttributes) {
				streamWriter.Write("; {0}=\"{1}\"", pair.Key, pair.Value);
			}

			streamWriter.WriteLine();
			foreach (string headerName in this.PartHeaders) {
				streamWriter.WriteLine("{0}: {1}", headerName, this.PartHeaders[headerName]);
			}

			streamWriter.WriteLine();
			streamWriter.Flush();
			this.Content.CopyTo(streamWriter.BaseStream);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				this.Content.Dispose();
			}
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void Invariant() {
			Contract.Invariant(!string.IsNullOrEmpty(this.ContentDisposition));
			Contract.Invariant(this.PartHeaders != null);
			Contract.Invariant(this.ContentAttributes != null);
		}
#endif
	}
}
