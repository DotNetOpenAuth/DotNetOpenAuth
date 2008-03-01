using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Hosting;
using System.IO;

namespace DotNetOpenId.Test.Hosting {
	class TestingWorkerRequest : SimpleWorkerRequest {
		public TestingWorkerRequest(string page, string query, string body, TextWriter output)
			: base(page, query, output) {
			this.body = body;
		}
		string body;
		public override bool IsEntireEntityBodyIsPreloaded() {
			return true;
		}
		public override byte[] GetPreloadedEntityBody() {
			return Encoding.ASCII.GetBytes(body);
		}
	}
}
