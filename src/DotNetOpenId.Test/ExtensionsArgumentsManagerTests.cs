using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class ExtensionsArgumentsManagerTests {
		[Test]
		public void CreateOutgoing() {
			var mgr = ExtensionArgumentsManager.CreateOutgoingExtensions();
			Assert.IsNotNull(mgr);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateIncomingNull() {
			ExtensionArgumentsManager.CreateIncomingExtensions(null);
		}

		[Test]
		public void CreateIncomingEmpty() {
			var mgr = ExtensionArgumentsManager.CreateIncomingExtensions(new Dictionary<string, string>());
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void WritingModeGetExtensionArguments() {
			var mgr = ExtensionArgumentsManager.CreateOutgoingExtensions();
			mgr.GetExtensionArguments("some");
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void WritingModeContainsExtension() {
			var mgr = ExtensionArgumentsManager.CreateOutgoingExtensions();
			mgr.ContainsExtension("some");
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ReadingModeAddExtension() {
			var mgr = ExtensionArgumentsManager.CreateIncomingExtensions(new Dictionary<string, string>());
			mgr.AddExtensionArguments("some", new Dictionary<string, string>());
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ReadingModeGetArgumentsToSend() {
			var mgr = ExtensionArgumentsManager.CreateIncomingExtensions(new Dictionary<string, string>());
			mgr.GetArgumentsToSend(true);
		}
		
		[Test]
		public void CreateIncomingSimpleNoExtensions() {
			var args = new Dictionary<string, string>() {
				{"arg1","val1"},
				{"openid.arg2","val2"},
			};
			ExtensionArgumentsManager.CreateIncomingExtensions(args);
		}

		[Test]
		public void CreateIncomingSimpleSomeExtensions() {
			var args = new Dictionary<string, string>() {
				{"arg1","val1"},
				{"arg2","val2"},
				{"openid.qq.k1", "v1"},
				{"openid.ns.qq", "QQExtTypeUri"},
				{"openid.ns.ss", "SSExtTypeUri"},
				{"openid.ss.k2", "v2"},
				{"openid.ns.mt", "MTExtTypeUri"},
			};
			var mgr = ExtensionArgumentsManager.CreateIncomingExtensions(args);
			Assert.IsTrue(mgr.ContainsExtension("QQExtTypeUri"));
			Assert.IsTrue(mgr.ContainsExtension("SSExtTypeUri"));
			Assert.IsFalse(mgr.ContainsExtension("MTExtTypeUri"), "A declared alias without any arguments should not be considered a present extension.");
			var qq = mgr.GetExtensionArguments("QQExtTypeUri");
			var ss = mgr.GetExtensionArguments("SSExtTypeUri");
			var mt = mgr.GetExtensionArguments("MTExtTypeUri");
			Assert.IsNotNull(qq);
			Assert.IsNotNull(ss);
			Assert.IsNull(mt);
			Assert.AreEqual(1, qq.Count);
			Assert.AreEqual(1, ss.Count);
			Assert.AreEqual("v1", qq["k1"]);
			Assert.AreEqual("v2", ss["k2"]);
		}

		[Test]
		public void GetArgumentsToSend() {
			var ext1args = new Dictionary<string, string> {
				{ "e1k1", "e1v1" },
				{ "e1k2", "e1v2" },
			};
			var ext2args = new Dictionary<string, string> {
				{ "e2k1", "e2v1" },
				{ "e2k2", "e2v2" },
			};
			var mgr = ExtensionArgumentsManager.CreateOutgoingExtensions();
			mgr.AddExtensionArguments("e1URI", ext1args);
			mgr.AddExtensionArguments("e2URI", ext2args);
			var results = mgr.GetArgumentsToSend(true);
			Assert.AreEqual(6, results.Count);
			var mgrRoundtrip = ExtensionArgumentsManager.CreateIncomingExtensions(results);
			Assert.IsTrue(mgrRoundtrip.ContainsExtension("e1URI"));
			Assert.IsTrue(mgrRoundtrip.ContainsExtension("e2URI"));
			Assert.AreEqual("e1v1", mgrRoundtrip.GetExtensionArguments("e1URI")["e1k1"]);
			Assert.AreEqual("e1v2", mgrRoundtrip.GetExtensionArguments("e1URI")["e1k2"]);
			Assert.AreEqual("e2v1", mgrRoundtrip.GetExtensionArguments("e2URI")["e2k1"]);
			Assert.AreEqual("e2v2", mgrRoundtrip.GetExtensionArguments("e2URI")["e2k2"]);
		}

		[Test]
		public void TestSregReadingAffinity() {
			// Construct a dictionary that doesn't define sreg but uses it.
			var args = new Dictionary<string, string>() {
				{"openid.sreg.nickname", "andy"},
			};
			IIncomingExtensions mgr = ExtensionArgumentsManager.CreateIncomingExtensions(args);
			Assert.IsTrue(mgr.ContainsExtension(QueryStringArgs.sreg_ns));
			Assert.AreEqual("andy", mgr.GetExtensionArguments(QueryStringArgs.sreg_ns)["nickname"]);
			// Now imagine that sreg was used explicitly by something else...
			args = new Dictionary<string,string>() {
				{"openid.sreg.nickname", "andy"},
				{"openid.ns.sreg", "someOtherNS"},
			};
			mgr = ExtensionArgumentsManager.CreateIncomingExtensions(args);
			Assert.IsFalse(mgr.ContainsExtension(QueryStringArgs.sreg_ns));
			Assert.AreEqual("andy", mgr.GetExtensionArguments("someOtherNS")["nickname"]);
		}

		[Test]
		public void TestSregWritingAffinity() {
			var args = new Dictionary<string, string>() {
				{"nickname", "andy"},
			};
			var mgr = ExtensionArgumentsManager.CreateOutgoingExtensions();
			mgr.AddExtensionArguments(QueryStringArgs.sreg_ns, args);
			var results = mgr.GetArgumentsToSend(true);
			Assert.IsTrue(results.ContainsKey("openid.ns.sreg"));
			Assert.AreEqual(QueryStringArgs.sreg_ns, results["openid.ns.sreg"]);
			Assert.IsTrue(results.ContainsKey("openid.sreg.nickname"));
			Assert.AreEqual("andy", results["openid.sreg.nickname"]);
		}

		[Test]
		public void TestWritingNoPrefix() {
			var args = new Dictionary<string, string>() {
				{"nickname", "andy"},
			};
			var mgr = ExtensionArgumentsManager.CreateOutgoingExtensions();
			mgr.AddExtensionArguments(QueryStringArgs.sreg_ns, args);
			var results = mgr.GetArgumentsToSend(false);
			Assert.IsTrue(results.ContainsKey("ns.sreg"));
			Assert.AreEqual(QueryStringArgs.sreg_ns, results["ns.sreg"]);
			Assert.IsTrue(results.ContainsKey("sreg.nickname"));
			Assert.AreEqual("andy", results["sreg.nickname"]);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ReadMultipleAliasesForOneNamespace() {
			// This scenario is called out in the spec and forbidden.
			var args = new Dictionary<string, string>() {
				{"openid.ns.qq1", "QQExtTypeUri"},
				{"openid.ns.qq2", "QQExtTypeUri"},
			};
			ExtensionArgumentsManager.CreateIncomingExtensions(args);
		}

		[Test]
		public void AddExtensionArgsTwice() {
			var args1 = new Dictionary<string, string>() {
				{"k1", "v1"},
			};
			var args2 = new Dictionary<string, string>() {
				{"k2", "v2"},
			};
			var mgr = ExtensionArgumentsManager.CreateOutgoingExtensions();
			mgr.AddExtensionArguments("extTypeURI", args1);
			mgr.AddExtensionArguments("extTypeURI", args2);
			var results = mgr.GetArgumentsToSend(false);
			Assert.AreEqual(3, results.Count);
		}
	}
}
