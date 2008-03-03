using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Consumer;

namespace DotNetOpenId.Test.Consumer {
	[TestFixture]
	public class NonceTest {
		[Test]
		public void DefaultCtor() {
			Nonce nonce = new Nonce();
			Assert.Less((DateTime.Now - nonce.CreationDate).TotalSeconds, 2);
			Assert.GreaterOrEqual(nonce.Age.TotalSeconds, 0);
			Assert.LessOrEqual(nonce.Age.TotalSeconds, 2);
			Assert.IsFalse(nonce.IsExpired);
			Assert.AreEqual(nonce.CreationDate + OpenIdConsumer.MaximumUserAgentAuthenticationTime, nonce.ExpirationDate);
			Assert.IsFalse(string.IsNullOrEmpty(nonce.Code));
		}

		[Test]
		public void RecreatedNonce() {
			string code = "somecode";
			TimeSpan age = OpenIdConsumer.MaximumUserAgentAuthenticationTime.Subtract(TimeSpan.FromSeconds(10));
			DateTime creationDate = DateTime.Now.Subtract(age);
			Nonce nonce = new Nonce(code, creationDate);
			Assert.AreEqual(code, nonce.Code);
			Assert.AreEqual(creationDate, nonce.CreationDate);
			Assert.Less(((DateTime.Now - age) - creationDate).TotalSeconds, 2);
			Assert.IsFalse(nonce.IsExpired);
		}

		[Test]
		public void ExpiredNonce() {
			string code = "somecode";
			TimeSpan age = OpenIdConsumer.MaximumUserAgentAuthenticationTime.Add(TimeSpan.FromSeconds(10));
			DateTime creationDate = DateTime.Now.Subtract(age);
			Nonce nonce = new Nonce(code, creationDate);
			Assert.AreEqual(code, nonce.Code);
			Assert.AreEqual(creationDate, nonce.CreationDate);
			Assert.Less(((DateTime.Now - age) - creationDate).TotalSeconds, 2);
			Assert.IsTrue(nonce.IsExpired);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void NullCode() {
			new Nonce(null, DateTime.Now);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void EmptyCode() {
			new Nonce(string.Empty, DateTime.Now);
		}

		[Test]
		public void EqualsTest() {
			Assert.AreNotEqual(new Nonce(), new Nonce());
			Nonce nonce1 = new Nonce();
			Nonce nonce2 = new Nonce(nonce1.Code, nonce1.CreationDate);
			Assert.AreEqual(nonce1, nonce2);
		}
	}
}
