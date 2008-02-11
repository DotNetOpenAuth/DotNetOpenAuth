using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;

namespace DotNetOpenId.Test {
    [TestFixture]
    public class TraceUtilTest {

        string sniffTrace(string tracemessage) {
            var sb = new StringBuilder();
            using (TraceListener tl = new TextWriterTraceListener(new StringWriter(sb))) {
                Trace.Listeners.Add(tl);
                try {
                    TraceUtil.ConsumerTrace(tracemessage);
                } finally {
                    Trace.Listeners.Remove(tl);
                }
            }
            return sb.ToString();
        }

        [Test]
        public void TraceTest() {
            Assert.IsTrue(sniffTrace("TESTTRACE").Contains("TESTTRACE"));
        }
    }
}
