using System.Collections.Generic;

namespace DotNetOpenAuth.Web
{
    public interface IOAuthDataProvider
    {
        string GetUserNameFromOAuth(string oAuthProvider, string oAuthId);

        void CreateOrUpdateOAuthAccount(string oAuthProvider, string oAuthId, string userName);

        bool DeleteOAuthAccount(string oAuthProvider, string oAuthId);

        ICollection<OAuthAccount> GetOAuthAccountsFromUserName(string userName);
    }
}