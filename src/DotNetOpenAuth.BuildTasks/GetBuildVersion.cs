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

			string commitId = string.Empty;

			// First try asking Git for the HEAD commit id
			try {
				string cmdPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
				ProcessStartInfo psi = new ProcessStartInfo(cmdPath, "/c git rev-parse HEAD");
				psi.WindowStyle = ProcessWindowStyle.Hidden;
				psi.RedirectStandardOutput = true;
				psi.UseShellExecute = false;
				Process git = Process.Start(psi);
				commitId = git.StandardOutput.ReadLine();
				git.WaitForExit();
				if (git.ExitCode != 0) {
					commitId = null;
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

			return commitId.Trim();
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
