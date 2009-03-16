namespace OpenIdProviderMvc.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;

	public interface IMembershipService {
		/// <summary>
		/// Gets the length of the min password.
		/// </summary>
		int MinPasswordLength { get; }

		/// <summary>
		/// Validates the user.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="password">The password.</param>
		/// <returns>Whether the given username and password is correct.</returns>
		bool ValidateUser(string userName, string password);

		/// <summary>
		/// Creates a new user account.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="password">The password.</param>
		/// <param name="email">The email.</param>
		/// <returns>The success or reason for failure of account creation.</returns>
		MembershipCreateStatus CreateUser(string userName, string password, string email);

		/// <summary>
		/// Changes the password for a user.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="oldPassword">The old password.</param>
		/// <param name="newPassword">The new password.</param>
		/// <returns>A value indicating whether the password change was successful.</returns>
		bool ChangePassword(string userName, string oldPassword, string newPassword);
	}
}
