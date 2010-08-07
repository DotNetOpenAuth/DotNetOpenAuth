namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Principal;
	using System.Web;

	/// <summary>
	/// Extension methods and other helpful utility methods.
	/// </summary>
	public static class Utilities {
		/// <summary>
		/// Gets the database entity representing the user identified by a given <see cref="IIdentity"/> instance.
		/// </summary>
		/// <param name="identity">The identity of the user.</param>
		/// <returns>
		/// The database object for that user; or <c>null</c> if the user could not
		/// be found or if <paramref name="identity"/> is <c>null</c> or represents an anonymous identity.
		/// </returns>
		public static User GetUser(this IIdentity identity) {
			if (identity == null || !identity.IsAuthenticated) {
				return null;
			}

			return Global.DataContext.Users.SingleOrDefault(user => user.OpenIDClaimedIdentifier == identity.Name);
		}
	}
}