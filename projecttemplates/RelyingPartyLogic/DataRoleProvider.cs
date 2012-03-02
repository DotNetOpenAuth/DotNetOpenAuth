//-----------------------------------------------------------------------
// <copyright file="DataRoleProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;

	public class DataRoleProvider : RoleProvider {
		public override string ApplicationName {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public override void AddUsersToRoles(string[] usernames, string[] roleNames) {
			var users = from token in Database.DataContext.AuthenticationTokens
						where usernames.Contains(token.ClaimedIdentifier)
						select token.User;
			var roles = from role in Database.DataContext.Roles
						where roleNames.Contains(role.Name, StringComparer.OrdinalIgnoreCase)
						select role;
			foreach (User user in users) {
				foreach (Role role in roles) {
					user.Roles.Add(role);
				}
			}
		}

		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) {
			var users = from token in Database.DataContext.AuthenticationTokens
						where usernames.Contains(token.ClaimedIdentifier)
						select token.User;
			var roles = from role in Database.DataContext.Roles
						where roleNames.Contains(role.Name, StringComparer.OrdinalIgnoreCase)
						select role;
			foreach (User user in users) {
				foreach (Role role in roles) {
					user.Roles.Remove(role);
				}
			}
		}

		public override void CreateRole(string roleName) {
			Database.DataContext.AddToRoles(new Role { Name = roleName });
		}

		/// <summary>
		/// Removes a role from the data source for the configured applicationName.
		/// </summary>
		/// <param name="roleName">The name of the role to delete.</param>
		/// <param name="throwOnPopulatedRole">If true, throw an exception if <paramref name="roleName"/> has one or more members and do not delete <paramref name="roleName"/>.</param>
		/// <returns>
		/// true if the role was successfully deleted; otherwise, false.
		/// </returns>
		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) {
			Role role = Database.DataContext.Roles.SingleOrDefault(r => r.Name == roleName);
			if (role == null) {
				return false;
			}

			if (throwOnPopulatedRole && role.Users.Count > 0) {
				throw new InvalidOperationException();
			}

			Database.DataContext.DeleteObject(roleName);
			return true;
		}

		/// <summary>
		/// Gets an array of user names in a role where the user name contains the specified user name to match.
		/// </summary>
		/// <param name="roleName">The role to search in.</param>
		/// <param name="usernameToMatch">The user name to search for.</param>
		/// <returns>
		/// A string array containing the names of all the users where the user name matches <paramref name="usernameToMatch"/> and the user is a member of the specified role.
		/// </returns>
		public override string[] FindUsersInRole(string roleName, string usernameToMatch) {
			return (from role in Database.DataContext.Roles
					where role.Name == roleName
					from user in role.Users
					from authTokens in user.AuthenticationTokens
					where authTokens.ClaimedIdentifier == usernameToMatch
					select authTokens.ClaimedIdentifier).ToArray();
		}

		public override string[] GetAllRoles() {
			return Database.DataContext.Roles.Select(role => role.Name).ToArray();
		}

		public override string[] GetRolesForUser(string username) {
			return (from authToken in Database.DataContext.AuthenticationTokens
					where authToken.ClaimedIdentifier == username
					from role in authToken.User.Roles
					select role.Name).ToArray();
		}

		public override string[] GetUsersInRole(string roleName) {
			return (from role in Database.DataContext.Roles
					where string.Equals(role.Name, roleName, StringComparison.OrdinalIgnoreCase)
					from user in role.Users
					from token in user.AuthenticationTokens
					select token.ClaimedIdentifier).ToArray();
		}

		public override bool IsUserInRole(string username, string roleName) {
			Role role = Database.DataContext.Roles.SingleOrDefault(r => string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase));
			if (role != null) {
				return role.Users.Any(user => user.AuthenticationTokens.Any(token => token.ClaimedIdentifier == username));
			}

			return false;
		}

		public override bool RoleExists(string roleName) {
			return Database.DataContext.Roles.Any(role => string.Equals(role.Name, roleName, StringComparison.OrdinalIgnoreCase));
		}
	}
}
