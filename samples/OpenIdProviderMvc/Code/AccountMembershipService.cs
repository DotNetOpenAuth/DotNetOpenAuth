namespace OpenIdProviderMvc.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Security;

	public class AccountMembershipService : IMembershipService {
		private MembershipProvider provider;

		public AccountMembershipService()
			: this(null) {
		}

		public AccountMembershipService(MembershipProvider provider) {
			this.provider = provider ?? Membership.Provider;
		}

		public int MinPasswordLength {
			get {
				return this.provider.MinRequiredPasswordLength;
			}
		}

		public bool ValidateUser(string userName, string password) {
			return this.provider.ValidateUser(userName, password);
		}

		public MembershipCreateStatus CreateUser(string userName, string password, string email) {
			MembershipCreateStatus status;
			this.provider.CreateUser(userName, password, email, null, null, true, null, out status);
			return status;
		}

		public bool ChangePassword(string userName, string oldPassword, string newPassword) {
			MembershipUser currentUser = this.provider.GetUser(userName, true /* userIsOnline */);
			return currentUser.ChangePassword(oldPassword, newPassword);
		}
	}
}
