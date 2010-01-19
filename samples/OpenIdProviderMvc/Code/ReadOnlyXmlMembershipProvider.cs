namespace OpenIdProviderMvc.Code {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Configuration.Provider;
	using System.Security.Permissions;
	using System.Web;
	using System.Web.Hosting;
	using System.Web.Security;
	using System.Xml;

	public class ReadOnlyXmlMembershipProvider : MembershipProvider {
		private Dictionary<string, MembershipUser> users;
		private string xmlFileName;

		// MembershipProvider Properties
		public override string ApplicationName {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public override bool EnablePasswordRetrieval {
			get { return false; }
		}

		public override bool EnablePasswordReset {
			get { return false; }
		}

		public override int MaxInvalidPasswordAttempts {
			get { throw new NotSupportedException(); }
		}

		public override int MinRequiredNonAlphanumericCharacters {
			get { throw new NotSupportedException(); }
		}

		public override int MinRequiredPasswordLength {
			get { throw new NotSupportedException(); }
		}

		public override int PasswordAttemptWindow {
			get { throw new NotSupportedException(); }
		}

		public override MembershipPasswordFormat PasswordFormat {
			get { throw new NotSupportedException(); }
		}

		public override string PasswordStrengthRegularExpression {
			get { throw new NotSupportedException(); }
		}

		public override bool RequiresQuestionAndAnswer {
			get { throw new NotSupportedException(); }
		}

		public override bool RequiresUniqueEmail {
			get { throw new NotSupportedException(); }
		}

		// MembershipProvider Methods
		public override void Initialize(string name, NameValueCollection config) {
			// Verify that config isn't null
			if (config == null) {
				throw new ArgumentNullException("config");
			}

			// Assign the provider a default name if it doesn't have one
			if (string.IsNullOrEmpty(name)) {
				name = "ReadOnlyXmlMembershipProvider";
			}

			// Add a default "description" attribute to config if the
			// attribute doesn't exist or is empty
			if (string.IsNullOrEmpty(config["description"])) {
				config.Remove("description");
				config.Add("description", "Read-only XML membership provider");
			}

			// Call the base class's Initialize method
			base.Initialize(name, config);

			// Initialize _XmlFileName and make sure the path
			// is app-relative
			string path = config["xmlFileName"];

			if (string.IsNullOrEmpty(path)) {
				path = "~/App_Data/Users.xml";
			}

			if (!VirtualPathUtility.IsAppRelative(path)) {
				throw new ArgumentException("xmlFileName must be app-relative");
			}

			string fullyQualifiedPath = VirtualPathUtility.Combine(
				VirtualPathUtility.AppendTrailingSlash(HttpRuntime.AppDomainAppVirtualPath),
				path);

			this.xmlFileName = HostingEnvironment.MapPath(fullyQualifiedPath);
			config.Remove("xmlFileName");

			// Make sure we have permission to read the XML data source and
			// throw an exception if we don't
			FileIOPermission permission = new FileIOPermission(FileIOPermissionAccess.Read, this.xmlFileName);
			permission.Demand();

			// Throw an exception if unrecognized attributes remain
			if (config.Count > 0) {
				string attr = config.GetKey(0);
				if (!string.IsNullOrEmpty(attr)) {
					throw new ProviderException("Unrecognized attribute: " + attr);
				}
			}
		}

		public override bool ValidateUser(string username, string password) {
			// Validate input parameters
			if (string.IsNullOrEmpty(username) ||
				string.IsNullOrEmpty(password)) {
				return false;
			}

			try {
				// Make sure the data source has been loaded
				this.ReadMembershipDataStore();

				// Validate the user name and password
				MembershipUser user;
				if (this.users.TryGetValue(username, out user)) {
					if (user.Comment == password) { // Case-sensitive
						// NOTE: A read/write membership provider
						// would update the user's LastLoginDate here.
						// A fully featured provider would also fire
						// an AuditMembershipAuthenticationSuccess
						// Web event
						return true;
					}
				}

				// NOTE: A fully featured membership provider would
				// fire an AuditMembershipAuthenticationFailure
				// Web event here
				return false;
			} catch (Exception) {
				return false;
			}
		}

		public override MembershipUser GetUser(string username, bool userIsOnline) {
			// Note: This implementation ignores userIsOnline

			// Validate input parameters
			if (string.IsNullOrEmpty(username)) {
				return null;
			}

			// Make sure the data source has been loaded
			this.ReadMembershipDataStore();

			// Retrieve the user from the data source
			MembershipUser user;
			if (this.users.TryGetValue(username, out user)) {
				return user;
			}

			return null;
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords) {
			// Note: This implementation ignores pageIndex and pageSize,
			// and it doesn't sort the MembershipUser objects returned

			// Make sure the data source has been loaded
			this.ReadMembershipDataStore();

			MembershipUserCollection users = new MembershipUserCollection();

			foreach (KeyValuePair<string, MembershipUser> pair in this.users) {
				users.Add(pair.Value);
			}

			totalRecords = users.Count;
			return users;
		}

		public override int GetNumberOfUsersOnline() {
			throw new NotSupportedException();
		}

		public override bool ChangePassword(string username, string oldPassword, string newPassword) {
			throw new NotSupportedException();
		}

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) {
			throw new NotSupportedException();
		}

		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status) {
			throw new NotSupportedException();
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData) {
			throw new NotSupportedException();
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) {
			throw new NotSupportedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {
			throw new NotSupportedException();
		}

		public override string GetPassword(string username, string answer) {
			throw new NotSupportedException();
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline) {
			throw new NotSupportedException();
		}

		public override string GetUserNameByEmail(string email) {
			throw new NotSupportedException();
		}

		public override string ResetPassword(string username, string answer) {
			throw new NotSupportedException();
		}

		public override bool UnlockUser(string userName) {
			throw new NotSupportedException();
		}

		public override void UpdateUser(MembershipUser user) {
			throw new NotSupportedException();
		}

		internal string GetSalt(string userName) {
			// This is just a sample with no database... a real web app MUST return 
			// a reasonable salt here and have that salt be persistent for each user.
			this.ReadMembershipDataStore();
			return this.users[userName].Email;
		}

		// Helper method
		private void ReadMembershipDataStore() {
			lock (this) {
				if (this.users == null) {
					this.users = new Dictionary<string, MembershipUser>(16, StringComparer.InvariantCultureIgnoreCase);
					XmlDocument doc = new XmlDocument();
					doc.Load(this.xmlFileName);
					XmlNodeList nodes = doc.GetElementsByTagName("User");

					foreach (XmlNode node in nodes) {
						// Yes, we're misusing some of these fields.  A real app would
						// have the right fields from a database to use.
						MembershipUser user = new MembershipUser(
							Name,                       // Provider name
							node["UserName"].InnerText, // Username
							null,                       // providerUserKey
							node["Salt"].InnerText,     // Email
							string.Empty,               // passwordQuestion
							node["Password"].InnerText, // Comment
							true,                       // isApproved
							false,                      // isLockedOut
							DateTime.Now,               // creationDate
							DateTime.Now,               // lastLoginDate
							DateTime.Now,               // lastActivityDate
							DateTime.Now, // lastPasswordChangedDate
							new DateTime(1980, 1, 1));  // lastLockoutDate

						this.users.Add(user.UserName, user);
					}
				}
			}
		}
	}
}