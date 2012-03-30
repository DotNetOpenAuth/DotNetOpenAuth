using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DotNetOpenAuth.BuildTasks {
	public class GetBuildVersion : Task {
		/// <summary>
		/// Initializes a new instance of the <see cref="GetBuildVersion"/> class.
		/// </summary>
		public GetBuildVersion() {
		}

		/// <summary>
		/// Gets the version string to use in the compiled assemblies.
		/// </summary>
		[Output]
		public string Version { get; private set; }

		/// <summary>
		/// Gets the version string to use in the official release name (lacks revision number).
		/// </summary>
		[Output]
		public string SimpleVersion { get; private set; }

		/// <summary>
		/// Gets or sets the major.minor version string.
		/// </summary>
		/// <value>
		/// The x.y string (no build number or revision number).
		/// </value>
		[Output]
		public string MajorMinorVersion { get; set; }

		/// <summary>
		/// Gets or sets the prerelease version, or empty if this is a final release.
		/// </summary>
		/// <value>
		/// The prerelease version.
		/// </value>
		[Output]
		public string PrereleaseVersion { get; set; }

		/// <summary>
		/// Gets or sets the version string to use for NuGet packages containing OAuth 2 components.
		/// </summary>
		[Output]
		public string OAuth2PackagesVersion { get; set; }

		/// <summary>
		/// Gets the Git revision control commit id for HEAD (the current source code version).
		/// </summary>
		[Output]
		public string GitCommitId { get; private set; }

		/// <summary>
		/// Gets the build number (JDate) for this version.
		/// </summary>
		[Output]
		public int BuildNumber { get; private set; }

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
				Version typedVersion;
				string prerelease, oauth2PackagesVersion;
				this.ReadVersionFromFile(out typedVersion, out prerelease, out oauth2PackagesVersion);
				this.PrereleaseVersion = prerelease;
				this.OAuth2PackagesVersion = oauth2PackagesVersion;
				this.SimpleVersion = typedVersion.ToString();
				this.MajorMinorVersion = new Version(typedVersion.Major, typedVersion.Minor).ToString();
				this.BuildNumber = this.CalculateJDate(DateTime.Now);

				var fullVersion = new Version(typedVersion.Major, typedVersion.Minor, typedVersion.Build, this.BuildNumber);
				this.Version = fullVersion.ToString();

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

			string commitId = string.Empty;

			// First try asking Git for the HEAD commit id
			try {
				string cmdPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
				var psi = new ProcessStartInfo(cmdPath, "/c git rev-parse HEAD") {
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					UseShellExecute = false
				};
				var git = Process.Start(psi);
				commitId = git.StandardOutput.ReadLine();
				git.WaitForExit();
				if (git.ExitCode != 0) {
					commitId = string.Empty;
				}
				if (commitId != null) {
					commitId = commitId.Trim();
					if (commitId.Length == 40) {
						return commitId;
					}
				}
			} catch (InvalidOperationException) {
			} catch (Win32Exception) {
			}

			// Failing being able to use the git command to figure out the HEAD commit ID, try the filesystem directly.
			try {
				string headContent = File.ReadAllText(Path.Combine(this.GitRepoRoot, @".git/HEAD")).Trim();
				if (headContent.StartsWith("ref:", StringComparison.Ordinal)) {
					string refName = headContent.Substring(5).Trim();
					string refPath = Path.Combine(this.GitRepoRoot, ".git/" + refName);
					if (File.Exists(refPath)) {
						commitId = File.ReadAllText(refPath).Trim();
					} else {
						string packedRefPath = Path.Combine(this.GitRepoRoot, ".git/packed-refs");
						string matchingLine = File.ReadAllLines(packedRefPath).FirstOrDefault(line => line.EndsWith(refName));
						if (matchingLine != null) {
							commitId = matchingLine.Substring(0, matchingLine.IndexOf(' '));
						}
					}
				} else {
					commitId = headContent;
				}
			} catch (FileNotFoundException) {
			} catch (DirectoryNotFoundException) {
			}

			commitId = commitId ?? String.Empty; // doubly-be sure it's not null, since in some error cases it can be.
			return commitId.Trim();
		}

		private void ReadVersionFromFile(out Version typedVersion, out string prereleaseVersion, out string oauth2PackagesVersion) {
			string[] lines = File.ReadAllLines(VersionFile);
			string versionLine = lines[0];
			prereleaseVersion = lines.Length >= 2 ? lines[1] : null;
			oauth2PackagesVersion = lines.Length >= 3 ? lines[2] : null;
			if (!String.IsNullOrEmpty(prereleaseVersion)) {
				if (!prereleaseVersion.StartsWith("-")) {
					// SemVer requires that prerelease suffixes begin with a hyphen, so add one if it's missing.
					prereleaseVersion = "-" + prereleaseVersion;
				}

				this.VerifyValidPrereleaseVersion(prereleaseVersion);
			}

			typedVersion = new Version(versionLine);
		}

		private int CalculateJDate(DateTime date) {
			int yearLastDigit = date.Year - 2000; // can actually be two digits in or after 2010
			DateTime firstOfYear = new DateTime(date.Year, 1, 1);
			int dayOfYear = (date - firstOfYear).Days + 1;
			int jdate = yearLastDigit * 1000 + dayOfYear;
			return jdate;
		}

		private void VerifyValidPrereleaseVersion(string prerelease) {
			if (prerelease[0] != '-') {
				throw new ArgumentOutOfRangeException("The prerelease string must begin with a hyphen.");
			}

			for (int i = 1; i < prerelease.Length; i++) {
				if (!char.IsLetterOrDigit(prerelease[i])) {
					throw new ArgumentOutOfRangeException("The prerelease string must be alphanumeric.");
				}
			}
		}
	}
}
