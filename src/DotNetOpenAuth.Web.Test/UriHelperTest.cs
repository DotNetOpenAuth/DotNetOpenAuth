using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenAuth.Web.Clients;

namespace DotNetOpenAuth.Web.Test
{
    [TestFixture]
    public class UriHelperTest
    {
        [TestCase]
        public void TestAttachQueryStringParameterMethod()
        {
            // Arrange
            string[] input = new string[]
                                  {
                                      "http://x.com",
                                      "https://xxx.com/one?s=123",
                                      "https://yyy.com/?s=6&u=a",
                                      "https://zzz.com/default.aspx?name=sd"
                                  };

            string[] expectedOutput = new string[]
                                          {
                                              "http://x.com/?s=awesome",
                                              "https://xxx.com/one?s=awesome",
                                              "https://yyy.com/?s=awesome&u=a",
                                              "https://zzz.com/default.aspx?name=sd&s=awesome"
                                          };

            for (int i = 0; i < input.Length; i++)
            {
                // Act
                var inputUrl = new Uri(input[i]);
                var outputUri = UriHelper.AttachQueryStringParameter(inputUrl, "s", "awesome");

                // Assert
                Assert.AreEqual(expectedOutput[i], outputUri.ToString());
            }
        }

        [TestCase]
        public void TestAppendQueryArguments()
        {
            // Arrange
            var builder = new UriBuilder("http://www.microsoft.com");

            // Act
            builder.AppendQueryArguments(
                new Dictionary<string, string> {{"one", "xxx"}, {"two", "yyy"}});

            // Assert
            if (builder.Port == 80)
            {
                // set port = -1 so that the display string doesn't contain port number
                builder.Port = -1;
            }
            string s = builder.ToString();
            Assert.AreEqual("http://www.microsoft.com/?one=xxx&two=yyy", s);
        }
    }
}
