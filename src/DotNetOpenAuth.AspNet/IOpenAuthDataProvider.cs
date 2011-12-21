
namespace DotNetOpenAuth.AspNet
{
    public interface IOpenAuthDataProvider
    {
        string GetUserNameFromOpenAuth(string openAuthProvider, string openAuthId);
    }
}