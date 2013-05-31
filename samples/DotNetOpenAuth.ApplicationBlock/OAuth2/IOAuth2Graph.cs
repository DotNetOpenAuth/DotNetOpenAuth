namespace DotNetOpenAuth.ApplicationBlock {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public enum HumanGender {
		/// <summary>
		/// The gender is unknown.
		/// </summary>
		Unknown,

		/// <summary>
		/// The gender is male.
		/// </summary>
		Male,

		/// <summary>
		/// The gender is female.
		/// </summary>
		Female,

		/// <summary>
		/// Hmmmm... What could this be?
		/// </summary>
		Other,
	}

	public interface IOAuth2Graph {
		string Id { get; }

		Uri Link { get; }

		string Name { get; }

		string FirstName { get; }

		string LastName { get; }

		string Gender { get; }

		string Locale { get; }

		DateTime? BirthdayDT { get; }

		string Email { get; }

		Uri AvatarUrl { get; }

		string UpdatedTime { get; }

		HumanGender GenderEnum { get; }
	}
}
