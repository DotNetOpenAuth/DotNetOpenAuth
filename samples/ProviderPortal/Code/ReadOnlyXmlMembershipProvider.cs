using System;
using System.Xml;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Security;
using System.Web.Hosting;
using System.Web.Management;
using System.Security.Permissions;
using System.Web;

public class ReadOnlyXmlMembershipProvider : MembershipProvider {
	private Dictionary<string, MembershipUser> _Users;
	private string _XmlFileName;

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
	public override void Initialize(string name,
		NameValueCollection config) {
		// Verify that config isn't null
		if (config == null)
			throw new ArgumentNullException("config");

		// Assign the provider a default name if it doesn't have one
		if (String.IsNullOrEmpty(name))
			name = "ReadOnlyXmlMembershipProvider";

		// Add a default "description" attribute to config if the
		// attribute doesn't exist or is empty
		if (string.IsNullOrEmpty(config["description"])) {
			config.Remove("description");
			config.Add("description",
				"Read-only XML membership provider");
		}

		// Call the base class's Initialize method
		base.Initialize(name, config);

		// Initialize _XmlFileName and make sure the path
		// is app-relative
		string path = config["xmlFileName"];

		if (String.IsNullOrEmpty(path))
			path = "~/App_Data/Users.xml";

		if (!VirtualPathUtility.IsAppRelative(path))
			throw new ArgumentException
				("xmlFileName must be app-relative");

		string fullyQualifiedPath = VirtualPathUtility.Combine
			(VirtualPathUtility.AppendTrailingSlash
			(HttpRuntime.AppDomainAppVirtualPath), path);

		_XmlFileName = HostingEnvironment.MapPath(fullyQualifiedPath);
		config.Remove("xmlFileName");

		// Make sure we have permission to read the XML data source and
		// throw an exception if we don't
		FileIOPermission permission =
			new FileIOPermission(FileIOPermissionAccess.Read,
			_XmlFileName);
		permission.Demand();

		// Throw an exception if unrecognized attributes remain
		if (config.Count > 0) {
			string attr = config.GetKey(0);
			if (!String.IsNullOrEmpty(attr))
				throw new ProviderException
					("Unrecognized attribute: " + attr);
		}
	}

	public override bool ValidateUser(string username, string password) {
		// Validate input parameters
		if (String.IsNullOrEmpty(username) ||
			String.IsNullOrEmpty(password))
			return false;

		try {
			// Make sure the data source has been loaded
			ReadMembershipDataStore();

			// Validate the user name and password
			MembershipUser user;
			if (_Users.TryGetValue(username, out user)) {
				if (user.Comment == password) // Case-sensitive
                {
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

	public override MembershipUser GetUser(string username,
		bool userIsOnline) {
		// Note: This implementation ignores userIsOnline

		// Validate input parameters
		if (String.IsNullOrEmpty(username))
			return null;

		// Make sure the data source has been loaded
		ReadMembershipDataStore();

		// Retrieve the user from the data source
		MembershipUser user;
		if (_Users.TryGetValue(username, out user))
			return user;

		return null;
	}

	public override MembershipUserCollection GetAllUsers(int pageIndex,
		int pageSize, out int totalRecords) {
		// Note: This implementation ignores pageIndex and pageSize,
		// and it doesn't sort the MembershipUser objects returned

		// Make sure the data source has been loaded
		ReadMembershipDataStore();

		MembershipUserCollection users =
			new MembershipUserCollection();

		foreach (KeyValuePair<string, MembershipUser> pair in _Users)
			users.Add(pair.Value);

		totalRecords = users.Count;
		return users;
	}

	public override int GetNumberOfUsersOnline() {
		throw new NotSupportedException();
	}

	public override bool ChangePassword(string username,
		string oldPassword, string newPassword) {
		throw new NotSupportedException();
	}

	public override bool
		ChangePasswordQuestionAndAnswer(string username,
		string password, string newPasswordQuestion,
		string newPasswordAnswer) {
		throw new NotSupportedException();
	}

	public override MembershipUser CreateUser(string username,
		string password, string email, string passwordQuestion,
		string passwordAnswer, bool isApproved, object providerUserKey,
		out MembershipCreateStatus status) {
		throw new NotSupportedException();
	}

	public override bool DeleteUser(string username,
		bool deleteAllRelatedData) {
		throw new NotSupportedException();
	}

	public override MembershipUserCollection
		FindUsersByEmail(string emailToMatch, int pageIndex,
		int pageSize, out int totalRecords) {
		throw new NotSupportedException();
	}

	public override MembershipUserCollection
		FindUsersByName(string usernameToMatch, int pageIndex,
		int pageSize, out int totalRecords) {
		throw new NotSupportedException();
	}

	public override string GetPassword(string username, string answer) {
		throw new NotSupportedException();
	}

	public override MembershipUser GetUser(object providerUserKey,
		bool userIsOnline) {
		throw new NotSupportedException();
	}

	public override string GetUserNameByEmail(string email) {
		throw new NotSupportedException();
	}

	public override string ResetPassword(string username,
		string answer) {
		throw new NotSupportedException();
	}

	public override bool UnlockUser(string userName) {
		throw new NotSupportedException();
	}

	public override void UpdateUser(MembershipUser user) {
		throw new NotSupportedException();
	}

	// Helper method
	private void ReadMembershipDataStore() {
		lock (this) {
			if (_Users == null) {
				_Users = new Dictionary<string, MembershipUser>
				   (16, StringComparer.InvariantCultureIgnoreCase);
				XmlDocument doc = new XmlDocument();
				doc.Load(_XmlFileName);
				XmlNodeList nodes = doc.GetElementsByTagName("User");

				foreach (XmlNode node in nodes) {
					MembershipUser user = new MembershipUser(
						Name,                       // Provider name
						node["UserName"].InnerText, // Username
						null,                       // providerUserKey
						null,                       // Email
						String.Empty,               // passwordQuestion
						node["Password"].InnerText, // Comment
						true,                       // isApproved
						false,                      // isLockedOut
						DateTime.Now,               // creationDate
						DateTime.Now,               // lastLoginDate
						DateTime.Now,               // lastActivityDate
						DateTime.Now, // lastPasswordChangedDate
						new DateTime(1980, 1, 1)    // lastLockoutDate
					);

					_Users.Add(user.UserName, user);
				}
			}
		}
	}
}
