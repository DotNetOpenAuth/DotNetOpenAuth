using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Security;
using DotNetOpenAuth.Web.Clients;
using Moq;
using NUnit.Framework;

namespace DotNetOpenAuth.Web.Test
{
    [TestFixture]
    public class OAuthWebSecurityTest
    {
        [TestCase]
        public void RegisterDataProviderThrowsOnNullValue()
        {
            Assert.Throws(typeof(ArgumentNullException), () => OAuthWebSecurity.RegisterDataProvider(null));
        }

        [TestCase]
        public void IsOAuthDataProviderIsFalseIfNotYetRegistered()
        {
            Assert.IsFalse(OAuthWebSecurity.IsOAuthDataProviderRegistered);
        }

        [TestCase]
        public void IsOAuthDataProviderIsTrueIfRegistered()
        {
            // Arrange
            OAuthWebSecurity.RegisterDataProvider(new Mock<IOAuthDataProvider>().Object);

            // Act & Assert
            Assert.IsTrue(OAuthWebSecurity.IsOAuthDataProviderRegistered);
        }

        [TestCase]
        public void RegisterDataProviderThrowsIfRegisterMoreThanOnce()
        {
            // Arrange
            OAuthWebSecurity.RegisterDataProvider(new Mock<IOAuthDataProvider>().Object);

            // Act & Assert
            Assert.Throws(
                typeof(InvalidOperationException),
                () => OAuthWebSecurity.RegisterDataProvider(new Mock<IOAuthDataProvider>().Object));
        }

        [TestCase]
        public void RegisterClientThrowsOnNullValue()
        {
            Assert.Throws(typeof(ArgumentNullException), () => OAuthWebSecurity.RegisterClient(null));
        }

        [TestCase]
        public void RegisterClientThrowsIfProviderNameIsEmpty()
        {
            // Arrange
            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns((string)null);

            // Act & Assert
            Assert.Throws(typeof(ArgumentException), () => OAuthWebSecurity.RegisterClient(client.Object), "Invalid provider name.");

            client.Setup(c => c.ProviderName).Returns("");

            // Act & Assert
            Assert.Throws(typeof(ArgumentException), () => OAuthWebSecurity.RegisterClient(client.Object), "Invalid provider name.");
        }

        [TestCase]
        public void RegisterClientThrowsRegisterMoreThanOneClientWithTheSameName()
        {
            // Arrange
            var client1 = new Mock<IAuthenticationClient>();
            client1.Setup(c => c.ProviderName).Returns("provider");

            var client2 = new Mock<IAuthenticationClient>();
            client2.Setup(c => c.ProviderName).Returns("provider");

            OAuthWebSecurity.RegisterClient(client1.Object);

            // Act & Assert
            Assert.Throws(
                typeof(ArgumentException),
                () => OAuthWebSecurity.RegisterClient(client2.Object),
                "Another service provider with the same name has already been registered.");
        }

        [TestCase]
        public void RegisterOAuthClient()
        {
            // Arrange
            var clients = new BuiltInOAuthClient[]
                              {
                                  BuiltInOAuthClient.Facebook,
                                  BuiltInOAuthClient.Twitter,
                                  BuiltInOAuthClient.LinkedIn,
                                  BuiltInOAuthClient.WindowsLive
                              };
            var clientNames = new string[]
                                  {
                                      "Facebook",
                                      "Twitter",
                                      "LinkedIn",
                                      "WindowsLive"
                                  };

            for (int i = 0; i < clients.Length; i++)
            {
                // Act
                OAuthWebSecurity.RegisterOAuthClient(clients[i], "key", "secret");

                var client = new Mock<IAuthenticationClient>();
                client.Setup(c => c.ProviderName).Returns(clientNames[i]);

                // Assert
                Assert.Throws(
                    typeof(ArgumentException),
                    () => OAuthWebSecurity.RegisterClient(client.Object),
                    "Another service provider with the same name has already been registered.");
            }
        }

        [TestCase]
        public void RegisterOpenIDClient()
        {
            // Arrange
            var clients = new BuiltInOpenIDClient[]
                              {
                                  BuiltInOpenIDClient.Google,
                                  BuiltInOpenIDClient.Yahoo
                              };
            var clientNames = new string[]
                                  {
                                      "Google",
                                      "Yahoo"
                                  };

            for (int i = 0; i < clients.Length; i++)
            {
                // Act
                OAuthWebSecurity.RegisterOpenIDClient(clients[i]);

                var client = new Mock<IAuthenticationClient>();
                client.Setup(c => c.ProviderName).Returns(clientNames[i]);

                // Assert
                Assert.Throws(
                    typeof(ArgumentException),
                    () => OAuthWebSecurity.RegisterClient(client.Object),
                    "Another service provider with the same name has already been registered.");
            }
        }

        [TestCase]
        public void RequestAuthenticationRedirectsToProviderWithNullReturnUrl()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request.ServerVariables).Returns(
                new NameValueCollection());
            context.Setup(c => c.Request.Url).Returns(new Uri("http://live.com/login.aspx"));
            context.Setup(c => c.Request.RawUrl).Returns("/login.aspx");

            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("windowslive");
            client.Setup(c => c.RequestAuthentication(
                                    context.Object,
                                    It.Is<Uri>(u => u.AbsoluteUri.Equals("http://live.com/login.aspx?__provider__=windowslive", StringComparison.OrdinalIgnoreCase))))
                  .Verifiable();

            OAuthWebSecurity.RegisterClient(client.Object);

            // Act
            OAuthWebSecurity.RequestAuthenticationCore(context.Object, "windowslive", null);

            // Assert
            client.Verify();
        }

        [TestCase]
        public void RequestAuthenticationRedirectsToProviderWithReturnUrl()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request.ServerVariables).Returns(
                new NameValueCollection());
            context.Setup(c => c.Request.Url).Returns(new Uri("http://live.com/login.aspx"));
            context.Setup(c => c.Request.RawUrl).Returns("/login.aspx");

            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("yahoo");
            client.Setup(c => c.RequestAuthentication(
                                    context.Object,
                                    It.Is<Uri>(u => u.AbsoluteUri.Equals("http://yahoo.com/?__provider__=yahoo", StringComparison.OrdinalIgnoreCase))))
                  .Verifiable();

            OAuthWebSecurity.RegisterClient(client.Object);

            // Act
            OAuthWebSecurity.RequestAuthenticationCore(context.Object, "yahoo", "http://yahoo.com");

            // Assert
            client.Verify();
        }

        [TestCase]
        public void VerifyAuthenticationSucceed()
        {
            // Arrange
            var queryStrings = new NameValueCollection();
            queryStrings.Add("__provider__", "facebook");

            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request.QueryString).Returns(queryStrings);

            var client = new Mock<IAuthenticationClient>(MockBehavior.Strict);
            client.Setup(c => c.ProviderName).Returns("facebook");
            client.Setup(c => c.VerifyAuthentication(context.Object)).Returns(new AuthenticationResult(true, "facebook", "123",
                                                                                                "super", null));

            var anotherClient = new Mock<IAuthenticationClient>(MockBehavior.Strict);
            anotherClient.Setup(c => c.ProviderName).Returns("twitter");
            anotherClient.Setup(c => c.VerifyAuthentication(context.Object)).Returns(AuthenticationResult.Failed);

            OAuthWebSecurity.RegisterClient(client.Object);
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthenticationCore(context.Object);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.AreEqual("facebook", result.Provider);
            Assert.AreEqual("123", result.ProviderUserId);
            Assert.AreEqual("super", result.UserName);
            Assert.IsNull(result.Error);
            Assert.IsNull(result.ExtraData);
        }

        [TestCase]
        public void VerifyAuthenticationFail()
        {
            // Arrange
            var queryStrings = new NameValueCollection();
            queryStrings.Add("__provider__", "twitter");

            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request.QueryString).Returns(queryStrings);

            var client = new Mock<IAuthenticationClient>(MockBehavior.Strict);
            client.Setup(c => c.ProviderName).Returns("facebook");
            client.Setup(c => c.VerifyAuthentication(context.Object)).Returns(new AuthenticationResult(true, "facebook", "123",
                                                                                                "super", null));

            var anotherClient = new Mock<IAuthenticationClient>(MockBehavior.Strict);
            anotherClient.Setup(c => c.ProviderName).Returns("twitter");
            anotherClient.Setup(c => c.VerifyAuthentication(context.Object)).Returns(AuthenticationResult.Failed);

            OAuthWebSecurity.RegisterClient(client.Object);
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthenticationCore(context.Object);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.AreEqual("twitter", result.Provider);
        }

        [TestCase]
        public void VerifyAuthenticationFailIfNoProviderInQueryString()
        {
            // Arrange
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request.QueryString).Returns(new NameValueCollection());

            var client = new Mock<IAuthenticationClient>(MockBehavior.Strict);
            client.Setup(c => c.ProviderName).Returns("facebook");

            var anotherClient = new Mock<IAuthenticationClient>(MockBehavior.Strict);
            anotherClient.Setup(c => c.ProviderName).Returns("twitter");

            OAuthWebSecurity.RegisterClient(client.Object);
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthenticationCore(context.Object);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.IsNull(result.Provider);
        }

        [TestCase]
        public void LoginSetAuthenticationTicketIfSuccessful()
        {
            // Arrange 
            var cookies = new HttpCookieCollection();
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request.IsSecureConnection).Returns(true);
            context.Setup(c => c.Response.Cookies).Returns(cookies);

            var dataProvider = new Mock<IOAuthDataProvider>(MockBehavior.Strict);
            dataProvider.Setup(p => p.GetUserNameFromOAuth("twitter", "12345")).Returns("hola");
            OAuthWebSecurity.RegisterDataProvider(dataProvider.Object);

            // Act
            bool successful = OAuthWebSecurity.LoginCore(context.Object, "twitter", "12345", createPersistentCookie: false);

            // Assert
            Assert.IsTrue(successful);

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
            Assert.AreEqual("hola", ticket.Name);
            Assert.AreEqual("OAuth", ticket.UserData);
            Assert.IsFalse(ticket.IsPersistent);
        }

        [TestCase]
        public void LoginFailIfUserIsNotFound()
        {
            // Arrange 
            var context = new Mock<HttpContextBase>();
            
            var dataProvider = new Mock<IOAuthDataProvider>();
            OAuthWebSecurity.RegisterDataProvider(dataProvider.Object);

            // Act
            bool successful = OAuthWebSecurity.LoginCore(context.Object, "twitter", "12345", createPersistentCookie: false);

            // Assert
            Assert.IsFalse(successful);
        }

        [TestCase]
        public void CreateOrUpdateAccountCallsOAuthDataProviderMethod()
        {
            // Arrange 
            var dataProvider = new Mock<IOAuthDataProvider>(MockBehavior.Strict);
            OAuthWebSecurity.RegisterDataProvider(dataProvider.Object);
            dataProvider.Setup(p => p.CreateOrUpdateOAuthAccount("twitter", "12345", "super")).Verifiable();

            // Act
            OAuthWebSecurity.CreateOrUpdateAccount("twitter", "12345", "super");

            // Assert
            dataProvider.Verify();
        }

        [TestCase]
        public void GetAccountsFromUserNameCallsOAuthDataProviderMethod()
        {
            // Arrange 
            var accounts = new OAuthAccount[]
                               {
                                   new OAuthAccount("twitter", "123"),
                                   new OAuthAccount("facebook", "abc"),
                                   new OAuthAccount("live", "xyz")
                               };

            var dataProvider = new Mock<IOAuthDataProvider>(MockBehavior.Strict);
            dataProvider.Setup(p => p.GetOAuthAccountsFromUserName("dotnetjunky")).Returns(accounts).Verifiable();

            OAuthWebSecurity.RegisterDataProvider(dataProvider.Object);

            // Act
            var retrievedAccounts = OAuthWebSecurity.GetAccountsFromUserName("dotnetjunky");

            // Assert
            CollectionAssert.AreEqual(retrievedAccounts, accounts);
        }

        [TestCase]
        public void DeleteAccountCallsOAuthDataProviderMethod()
        {
            // Arrange 
            var dataProvider = new Mock<IOAuthDataProvider>(MockBehavior.Strict);
            OAuthWebSecurity.RegisterDataProvider(dataProvider.Object);
            dataProvider.Setup(p => p.DeleteOAuthAccount("linkedin", "423432")).Returns(true).Verifiable();

            // Act
            OAuthWebSecurity.DeleteAccount("linkedin", "423432");

            // Assert
            dataProvider.Verify();
        }

        [TestCase]
        public void GetOAuthClientReturnsTheCorrectClient()
        {
            // Arrange
            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("facebook");
            OAuthWebSecurity.RegisterClient(client.Object);

            var anotherClient = new Mock<IAuthenticationClient>();
            anotherClient.Setup(c => c.ProviderName).Returns("hulu");
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act
            var expectedClient = OAuthWebSecurity.GetOAuthClient("facebook");

            // Assert
            Assert.AreSame(expectedClient, client.Object);
        }

        [TestCase]
        public void GetOAuthClientThrowsIfClientIsNotFound()
        {
            // Arrange
            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("facebook");
            OAuthWebSecurity.RegisterClient(client.Object);

            var anotherClient = new Mock<IAuthenticationClient>();
            anotherClient.Setup(c => c.ProviderName).Returns("hulu");
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => OAuthWebSecurity.GetOAuthClient("live"), 
                "A service provider could not be found by the specified name.");
        }

        [TestCase]
        public void TryGetOAuthClientSucceeds()
        {
            // Arrange
            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("facebook");
            OAuthWebSecurity.RegisterClient(client.Object);

            var anotherClient = new Mock<IAuthenticationClient>();
            anotherClient.Setup(c => c.ProviderName).Returns("hulu");
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act
            IAuthenticationClient expectedClient;
            bool result = OAuthWebSecurity.TryGetOAuthClient("facebook", out expectedClient);

            // Assert
            Assert.AreSame(expectedClient, client.Object);
            Assert.IsTrue(result);
        }

        [TestCase]
        public void TryGetOAuthClientFail()
        {
            // Arrange
            var client = new Mock<IAuthenticationClient>();
            client.Setup(c => c.ProviderName).Returns("facebook");
            OAuthWebSecurity.RegisterClient(client.Object);

            var anotherClient = new Mock<IAuthenticationClient>();
            anotherClient.Setup(c => c.ProviderName).Returns("hulu");
            OAuthWebSecurity.RegisterClient(anotherClient.Object);

            // Act
            IAuthenticationClient expectedClient;
            bool result = OAuthWebSecurity.TryGetOAuthClient("live", out expectedClient);

            // Assert
            Assert.IsNull(expectedClient);
            Assert.IsFalse(result);
        }

        [TearDown]
        public void Cleanup()
        {
            OAuthWebSecurity.ClearDataProvider();
            OAuthWebSecurity.ClearProviders();
        }
    }
}
