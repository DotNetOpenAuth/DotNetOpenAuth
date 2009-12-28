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
		/// Gets the Git revision control commit id for HEAD (the current source code version).
		/// </summary>
		[Output]
		public string GitCommitId { get; private set; }

		/// <summary>
		/// The file that contains the version base (Major.Minor.Build) to use.
		/// </summary>
		[Required]
		public string VersionFile { get; set; }

		/// <summary>
		/// Gets or sets the parent directory of the .git directory.
		/// </summary>
		public string GitRepoRoot { get; set; }

		public override bool Execute() {
			try {
				Version typedVersion = ReadVersionFromFile();
				typedVersion = new Version(typedVersion.Major, typedVersion.Minor, typedVersion.Build, CalculateJDate(DateTime.Now));
				Version = typedVersion.ToString();

				this.GitCommitId = GetGitHeadCommitId();
			} catch (ArgumentOutOfRangeException ex) {
				Log.LogErrorFromException(ex);
				return false;
			}

			return true;
		}

		private string GetGitHeadCommitId() {
			if (string.IsNullOrEmpty(this.GitRepoRoot)) {
				return string.Empty;
			}

			string headContent = string.Empty;
			try {
				headContent = File.ReadAllText(Path.Combine(this.GitRepoRoot, @".git/HEAD")).Trim();
				if (headContent.StartsWith("ref:", StringComparison.Ordinal)) {
					string refName = headContent.Substring(5).Trim();
					headContent = File.ReadAllText(Path.Combine(this.GitRepoRoot, @".git/" + refName)).Trim();
				}
			} catch (FileNotFoundException) {
			} catch (DirectoryNotFoundException) {
			}

			if (string.IsNullOrEmpty(headContent)) {
				Log.LogWarning("Unable to determine the git HEAD commit ID to use for informational version number.");
			}

			return headContent.Trim();
		}

		private Version ReadVersionFromFile() {
			string[] lines = File.ReadAllLines(VersionFile);
			string versionLine = lines[0];
			return new Version(versionLine);
		}

		private int CalculateJDate(DateTime date) {
			int yearLastDigit = date.Year - 2000; // can actually be two digits in or after 2010
			DateTime firstOfYear = new DateTime(date.Year, 1, 1);
			int dayOfYear = (date - firstOfYear).Days + 1;
			int jdate = yearLastDigit * 1000 + dayOfYear;
			return jdate;
		}
	}
}
