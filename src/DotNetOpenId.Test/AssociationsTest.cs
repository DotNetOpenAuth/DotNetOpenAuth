using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Threading;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class AssociationsTest {

		Associations assocs;
		[SetUp]
		public void SetUp() {
			assocs = new Associations();
		}

		[Test]
		public void GetNonexistentHandle() {
			Assert.IsNull(assocs.Get("someinvalidhandle"));
		}

		[Test]
		public void RemoveNonexistentHandle() {
			Assert.IsFalse(assocs.Remove("someinvalidhandle"));
		}

		[Test]
		public void HandleLifecycle() {
			Association a = new HmacSha1Association("somehandle", new byte[0], TimeSpan.FromDays(1));
			assocs.Set(a);
			Assert.AreSame(a, assocs.Get(a.Handle));
			Assert.IsTrue(assocs.Remove(a.Handle));
			Assert.IsNull(assocs.Get(a.Handle));
			Assert.IsFalse(assocs.Remove(a.Handle));
		}

		[Test]
		public void Best() {
			Association a = new HmacSha1Association("h1", new byte[0], TimeSpan.FromHours(1));
			Association b = new HmacSha1Association("h2", new byte[0], TimeSpan.FromHours(1));

			assocs.Set(a);
			assocs.Set(b);

			// make b the best by making a older
			a.Issued -= TimeSpan.FromHours(1);
			Assert.AreSame(b, assocs.Best);
			// now make a the best
			b.Issued -= TimeSpan.FromHours(2);
			Assert.AreSame(a, assocs.Best);
		}
	}
}
