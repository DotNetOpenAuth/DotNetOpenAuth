using System.Diagnostics.CodeAnalysis;

namespace DotNetOpenAuth.Web
{
    /// <summary>
    /// Represents built in OAuth clients.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OAuth")]
    public enum BuiltInOAuthClient
    {
        Twitter,
        Facebook,
        LinkedIn,
        WindowsLive
    }
}
