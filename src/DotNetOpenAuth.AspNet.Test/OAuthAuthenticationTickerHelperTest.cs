//-----------------------------------------------------------------------
// <copyright file="OAuthAuthenticationTickerHelperTest.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Web {
	using System;
	using System.Web;
	using System.Web.Security;
	using DotNetOpenAuth.AspNet;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class OAuthAuthenticationTickerHelperTest {
		[TestCase]
		public void SetAuthenticationTicketSetCookieOnHttpResponseWithPersistentSet() {
			this.SetAuthenticationTicketSetCookieOnHttpResponse(isPersistent: true);
		}

		[TestCase]
		public void SetAuthenticationTicketSetCookieOnHttpResponseWithPersistentNotSet() {
			this.SetAuthenticationTicketSetCookieOnHttpResponse(isPersistent: false);
		}

		[TestCase]
		public void IsOAuthAuthenticationTicketReturnsTrueIfCookieIsPresent() {
			// Arrange
			var ticket = new FormsAuthenticationTicket(
							  2,
							  "username",
							  DateTime.Now,
							  DateTime.Now.Add(FormsAuthentication.Timeout),
							  false,
							  "OAuth",
							  FormsAuthentication.FormsCookiePath);

			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
			var cookies = new HttpCookieCollection { cookie };

			var context = new Mock<HttpContextBase>();
			context.Setup(c => c.Request.Cookies).Returns(cookies);

			// Act
			bool result = OpenAuthAuthenticationTicketHelper.IsValidAuthenticationTicket(context.Object);

			// Assert
			Assert.IsTrue(result);
		}

		[TestCase]
		public void IsOAuthAuthenticationTicketReturnsFalseIfCookieIsNotPresent() {
			// Arrange
			var context = new Mock<HttpContextBase>();
			context.Setup(c => c.Request.Cookies).Returns(new HttpCookieCollection());

			// Act
			bool result = OpenAuthAuthenticationTicketHelper.IsValidAuthenticationTicket(context.Object);

			// Assert
			Assert.IsFalse(result);
		}

		[TestCase]
		public void IsOAuthAuthenticationTicketReturnsFalseIfCookieIsPresentButDoesNotHaveOAuthData() {
			// Arrange
			var ticket = new FormsAuthenticationTicket(
							  2,
							  "username",
							  DateTime.Now,
							  DateTime.Now.Add(FormsAuthentication.Timeout),
							  false,
							  null,
							  FormsAuthentication.FormsCookiePath);

			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
			var cookies = new HttpCookieCollection { cookie };

			var context = new Mock<HttpContextBase>();
			context.Setup(c => c.Request.Cookies).Returns(cookies);

			// Act
			bool result = OpenAuthAuthenticationTicketHelper.IsValidAuthenticationTicket(context.Object);

			// Assert
			Assert.IsFalse(result);
		}

		[TestCase]
		public void IsOAuthAuthenticationTicketReturnsFalseIfCookieIsPresentButDoesNotHaveCorrectName() {
			// Arrange
			var response = new Mock<HttpResponseBase>();

			var ticket = new FormsAuthenticationTicket(
							  2,
							  "username",
							  DateTime.Now,
							  DateTime.Now.Add(FormsAuthentication.Timeout),
							  false,
							  "OAuth",
							  FormsAuthentication.FormsCookiePath);

			var cookie = new HttpCookie("random cookie name", FormsAuthentication.Encrypt(ticket));
			var cookies = new HttpCookieCollection { cookie };

			var context = new Mock<HttpContextBase>();
			context.Setup(c => c.Request.Cookies).Returns(cookies);

			// Act
			bool result = OpenAuthAuthenticationTicketHelper.IsValidAuthenticationTicket(context.Object);

			// Assert
			Assert.IsFalse(result);
		}

		private void SetAuthenticationTicketSetCookieOnHttpResponse(bool isPersistent) {
			// Arrange
			var cookies = new HttpCookieCollection();

			var context = new Mock<HttpContextBase>();
			context.Setup(c => c.Request.IsSecureConnection).Returns(true);
			context.Setup(c => c.Response.Cookies).Returns(cookies);

			// Act
			OpenAuthAuthenticationTicketHelper.SetAuthenticationTicket(context.Object, "user", isPersistent);

			// Assert
			Assert.AreEqual(1, cookies.Count);
			HttpCookie addedCookie = cookies[0];

			Assert.AreEqual(FormsAuthentication.FormsCookieName, addedCookie.Name);
			Assert.IsTrue(addedCookie.HttpOnly);
			Assert.AreEqual("/", addedCookie.Path);
			Assert.IsFalse(addedCookie.Secure);
			Assert.IsNotNullOrEmpty(addedCookie.Value);

			FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(addedCookie.Value);
			Assert.NotNull(ticket);
			Assert.AreEqual(2, ticket.Version);
			Assert.AreEqual("user", ticket.Name);
			Assert.AreEqual("OAuth", ticket.UserData);
			Assert.AreEqual(isPersistent, ticket.IsPersistent);
		}
	}
}
