using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Hosting;
using System.IO;

namespace DotNetOpenId.Test.Hosting {
	class TestingWorkerRequest : SimpleWorkerRequest {
		public TestingWorkerRequest(string page, string query, Stream entityStream, TextWriter output)
			: base(page, query, output) {
			this.entityStream = entityStream;
		}
		Stream entityStream;
		public override bool IsEntireEntityBodyIsPreloaded() {
			return false;
		}
		public override int ReadEntityBody(byte[] buffer, int size) {
			return entityStream.Read(buffer, 0, size);
		}
		public override int ReadEntityBody(byte[] buffer, int offset, int size) {
			return entityStream.Read(buffer, offset, size);
		}
	}
}
