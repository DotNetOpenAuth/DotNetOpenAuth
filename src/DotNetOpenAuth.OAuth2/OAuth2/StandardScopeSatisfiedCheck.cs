//-----------------------------------------------------------------------
// <copyright file="StandardScopeSatisfiedCheck.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System.Collections.Generic;
	using Validation;

	/// <summary>
	/// The default scope superset checker, which assumes that no scopes overlap.
	/// </summary>
	internal class StandardScopeSatisfiedCheck : IScopeSatisfiedCheck {
		/// <summary>
		/// Checks whether the granted scope is a superset of the required scope.
		/// </summary>
		/// <param name="requiredScope">The set of strings that the resource server demands in an access token's scope in order to complete some operation.</param>
		/// <param name="grantedScope">The set of strings that define the scope within an access token that the client is authorized to.</param>
		/// <returns><c>true</c> if <paramref name="grantedScope"/> is a superset of <paramref name="requiredScope"/> to allow the request to proceed; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// The default reasonable implementation of this is:
		/// <code>
		///     return <paramref name="grantedScope"/>.IsSupersetOf(<paramref name="requiredScope"/>);
		/// </code>
		/// <para>In some advanced cases it may not be so simple.  One case is that there may be a string that aggregates the capabilities of several others
		/// in order to simplify common scenarios.  For example, the scope "ReadAll" may represent the same authorization as "ReadProfile", "ReadEmail", and 
		/// "ReadFriends".
		/// </para>
		/// <para>Great care should be taken in implementing this method as this is a critical security module for the authorization and resource servers.</para>
		/// </remarks>
		public bool IsScopeSatisfied(HashSet<string> requiredScope, HashSet<string> grantedScope) {
			Requires.NotNull(requiredScope, "requiredScope");
			Requires.NotNull(grantedScope, "grantedScope");
			return grantedScope.IsSupersetOf(requiredScope);
		}
	}
}
