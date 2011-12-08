using System;
using System.Collections.ObjectModel;

namespace DotNetOpenAuth.Web.Clients
{
    /// <summary>
    /// A collection to store instances of IAuthenticationClient by keying off ProviderName.
    /// </summary>
    internal sealed class AuthenticationClientCollection : KeyedCollection<string, IAuthenticationClient>
    {
        public AuthenticationClientCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        protected override string GetKeyForItem(IAuthenticationClient item)
        {
            return item.ProviderName;
        }
    }
}
