using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

namespace DotNetOpenAuth.BuildTasks {
	public class GetBuildVersion : Task {

		/// <summary>
		/// Gets the version string to use in the compiled assemblies.
		/// </summary>
		[Output]
		public string Version { get; private set; }

		/// <summary>
		/// The file that contains the version base (Major.Minor.Build) to use.
		/// </summary>
		[Required]
		public string VersionFile { get; set; }

		public override bool Execute() {
			try {
				Version typedVersion = ReadVersionFromFile();
				typedVersion = new Version(typedVersion.Major, typedVersion.Minor, typedVersion.Build, CalculateJDate(DateTime.Now));
				Version = typedVersion.ToString();
			} catch (ArgumentOutOfRangeException ex) {
				Log.LogErrorFromException(ex);
				return false;
			}

			return true;
		}

		private Version ReadVersionFromFile() {
			string[] lines = File.ReadAllLines(VersionFile);
			string versionLine = lines[0];
			return new Version(versionLine);
		}

		private int CalculateJDate(DateTime date) {
			int yearLastDigit = date.Year % 10;
			DateTime firstOfYear = new DateTime(date.Year, 1, 1);
			int dayOfYear = (date - firstOfYear).Days + 1;
			int jdate = yearLastDigit * 1000 + dayOfYear;
			return jdate;
		}
	}
}
