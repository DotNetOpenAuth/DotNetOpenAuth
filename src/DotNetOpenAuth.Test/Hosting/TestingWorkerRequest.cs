//-----------------------------------------------------------------------
// <copyright file="TestingWorkerRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Hosting {
	using System;
	using System.IO;
	using System.Net;
	using System.Web.Hosting;

	/// <summary>
	/// Processes individual incoming ASP.NET requests.
	/// </summary>
	internal class TestingWorkerRequest : SimpleWorkerRequest {
		private Stream entityStream;

		private HttpListenerContext context;

		private TextWriter writer;

		public TestingWorkerRequest(string page, string query, Stream entityStream, TextWriter writer)
			: base(page, query, writer) {
			this.entityStream = entityStream;
			this.writer = writer;
		}

		public TestingWorkerRequest(HttpListenerContext context, TextWriter output)
			: base(context.Request.Url.LocalPath.TrimStart('/'), context.Request.Url.Query, output) {
			this.entityStream = context.Request.InputStream;
			this.context = context;
			this.writer = output;
		}

		public override string GetFilePath() {
			string filePath = this.context.Request.Url.LocalPath.Replace("/", "\\");
			if (filePath.EndsWith("\\", StringComparison.Ordinal)) {
				filePath += "default.aspx";
			}
			return filePath;
		}

		public override int GetLocalPort() {
			return this.context.Request.Url.Port;
		}

		public override string GetServerName() {
			return this.context.Request.Url.Host;
		}

		public override string GetQueryString() {
			return this.context.Request.Url.Query.TrimStart('?');
		}

		public override string GetHttpVerbName() {
			return this.context.Request.HttpMethod;
		}

		public override string GetLocalAddress() {
			return this.context.Request.LocalEndPoint.Address.ToString();
		}

		public override string GetHttpVersion() {
			return "HTTP/1.1";
		}

		public override string GetProtocol() {
			return this.context.Request.Url.Scheme;
		}

		public override string GetRawUrl() {
			return this.context.Request.RawUrl;
		}

		public override int GetTotalEntityBodyLength() {
			return (int)this.context.Request.ContentLength64;
		}

		public override string GetKnownRequestHeader(int index) {
			return this.context.Request.Headers[GetKnownRequestHeaderName(index)];
		}

		public override string GetUnknownRequestHeader(string name) {
			return this.context.Request.Headers[name];
		}

		public override bool IsEntireEntityBodyIsPreloaded() {
			return false;
		}

		public override int ReadEntityBody(byte[] buffer, int size) {
			return this.entityStream.Read(buffer, 0, size);
		}

		public override int ReadEntityBody(byte[] buffer, int offset, int size) {
			return this.entityStream.Read(buffer, offset, size);
		}

		public override void SendCalculatedContentLength(int contentLength) {
			this.context.Response.ContentLength64 = contentLength;
		}

		public override void SendStatus(int statusCode, string statusDescription) {
			if (this.context != null) {
				this.context.Response.StatusCode = statusCode;
				this.context.Response.StatusDescription = statusDescription;
			}
		}

		public override void SendKnownResponseHeader(int index, string value) {
			if (this.context != null) {
				this.context.Response.Headers[(HttpResponseHeader)index] = value;
			}
		}

		public override void SendUnknownResponseHeader(string name, string value) {
			if (this.context != null) {
				this.context.Response.Headers[name] = value;
			}
		}
	}
}
