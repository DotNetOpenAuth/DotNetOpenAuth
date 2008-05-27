using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Hosting;
using System.IO;
using System.Diagnostics;
using System.Net;

namespace DotNetOpenId.Test.Hosting {
	class TestingWorkerRequest : SimpleWorkerRequest {
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

		Stream entityStream;
		HttpListenerContext context;
		TextWriter writer;
		public override string GetFilePath() {
			string filePath = context.Request.Url.LocalPath.Replace("/", "\\");
			if (filePath.EndsWith("\\", StringComparison.Ordinal))
				filePath += "default.aspx";
			return filePath;
		}
		public override int GetLocalPort() {
			return context.Request.Url.Port;
		}
		public override string GetServerName() {
			return context.Request.Url.Host;
		}
		public override string GetQueryString() {
			return context.Request.Url.Query.TrimStart('?');
		}
		public override string GetHttpVerbName() {
			return context.Request.HttpMethod;
		}
		public override string GetLocalAddress() {
			return context.Request.LocalEndPoint.Address.ToString();
		}
		public override string GetHttpVersion() {
			return "HTTP/1.1";
		}
		public override string GetProtocol() {
			return context.Request.Url.Scheme;
		}
		public override string GetRawUrl() {
			return context.Request.RawUrl;
		}
		public override int GetTotalEntityBodyLength() {
			return (int)context.Request.ContentLength64;
		}
		public override string GetKnownRequestHeader(int index) {
			return context.Request.Headers[GetKnownRequestHeaderName(index)];
		}
		public override string GetUnknownRequestHeader(string name) {
			return context.Request.Headers[name];
		}
		public override bool IsEntireEntityBodyIsPreloaded() {
			return false;
		}
		public override int ReadEntityBody(byte[] buffer, int size) {
			return entityStream.Read(buffer, 0, size);
		}
		public override int ReadEntityBody(byte[] buffer, int offset, int size) {
			return entityStream.Read(buffer, offset, size);
		}
		public override void SendCalculatedContentLength(int contentLength) {
			context.Response.ContentLength64 = contentLength;
		}
		public override void SendStatus(int statusCode, string statusDescription) {
			if (context != null) {
				context.Response.StatusCode = statusCode;
				context.Response.StatusDescription = statusDescription;
			}
		}
		public override void SendKnownResponseHeader(int index, string value) {
			if (context != null) {
				context.Response.Headers[(HttpResponseHeader)index] = value;
			}
		}
		public override void SendUnknownResponseHeader(string name, string value) {
			if (context != null) {
				context.Response.Headers[name] = value;
			}
		}
	}
}
