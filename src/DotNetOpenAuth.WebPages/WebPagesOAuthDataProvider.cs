using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using DotNetOpenAuth.Web;
using WebMatrix.WebData;

namespace DotNetOpenAuth.WebPages
{
    internal class WebPagesOAuthDataProvider : IOAuthDataProvider
    {
        public WebPagesOAuthDataProvider()
        {
        }

        private static ExtendedMembershipProvider VerifyProvider()
        {
            var provider = Membership.Provider as ExtendedMembershipProvider;
            if (provider == null)
            {
                throw new InvalidOperationException();
            }
            return provider;
        }

        public string GetUserNameFromOAuth(string oAuthProvider, string oAuthId)
        {
            ExtendedMembershipProvider provider = VerifyProvider();

            int userId = provider.GetUserIdFromOAuth(oAuthProvider, oAuthId);
            if (userId == -1) {
                return null;
            }

            return provider.GetUserNameFromId(userId);
        }

        public void CreateOrUpdateOAuthAccount(string oAuthProvider, string oAuthId, string username)
        {
            ExtendedMembershipProvider provider = VerifyProvider();
            provider.CreateOrUpdateOAuthAccount(oAuthProvider, oAuthId, username);
        }

        public bool DeleteOAuthAccount(string oAuthProvider, string oAuthId)
        {
            ExtendedMembershipProvider provider = VerifyProvider();

            string username = GetUserNameFromOAuth(oAuthProvider, oAuthId);
            if (String.IsNullOrEmpty(username))
            {
                // account doesn't exist
                return false;
            }

            provider.DeleteOAuthAccount(oAuthProvider, oAuthId);
            return true;
        }

        public ICollection<OAuthAccount> GetOAuthAccountsFromUserName(string userName)
        {
            ExtendedMembershipProvider provider = VerifyProvider();
            return provider.GetAccountsForUser(userName).Select(p => new OAuthAccount(p.Provider, p.ProviderUserId)).ToList();
        }
    }
}