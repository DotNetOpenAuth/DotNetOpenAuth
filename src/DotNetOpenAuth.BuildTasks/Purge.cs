//-----------------------------------------------------------------------
// <copyright file="Purge.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// Purges directory trees of all directories and files that are not on a whitelist.
	/// </summary>
	/// <remarks>
	/// This task performs a function similar to robocopy's /MIR switch, except that
	/// this task does not require that an entire directory tree be used as the source
	/// in order to purge old files from the destination.
	/// </remarks>
	public class Purge : Task {
		/// <summary>
		/// Initializes a new instance of the <see cref="Purge"/> class.
		/// </summary>
		public Purge() {
			this.PurgeEmptyDirectories = true;
		}

		/// <summary>
		/// Gets or sets the root directories to purge.
		/// </summary>
		/// <value>The directories.</value>
		[Required]
		public string[] Directories { get; set; }

		/// <summary>
		/// Gets or sets the files that should be NOT be purged.
		/// </summary>
		[Required]
		public ITaskItem[] IntendedFiles { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether empty directories will be deleted.
		/// </summary>
		/// <value>
		/// 	The default value is <c>true</c>.
		/// </value>
		public bool PurgeEmptyDirectories { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		public override bool Execute() {
			HashSet<string> intendedFiles = new HashSet<string>(this.IntendedFiles.Select(file => file.GetMetadata("FullPath")), StringComparer.OrdinalIgnoreCase);

			foreach (string directory in this.Directories.Select(dir => NormalizePath(dir)).Where(dir => Directory.Exists(dir))) {
				foreach (string existingFile in Directory.GetFiles(directory, "*", SearchOption.AllDirectories)) {
					if (!intendedFiles.Contains(existingFile)) {
						this.Log.LogWarning("Purging file \"{0}\".", existingFile);
						File.Delete(existingFile);
					}
				}

				if (this.PurgeEmptyDirectories) {
					foreach (string subdirectory in Directory.GetDirectories(directory, "*", SearchOption.AllDirectories)) {
						// We have to check for the existance of the directory because it MAY be
						// a descendent of a directory we already deleted in this loop.
						if (Directory.Exists(subdirectory)) {
							if (Directory.GetDirectories(subdirectory).Length == 0 && Directory.GetFiles(subdirectory).Length == 0) {
								this.Log.LogWarning("Purging empty directory \"{0}\".", subdirectory);
								Directory.Delete(subdirectory);
							}
						}
					}
				}
			}

			return !this.Log.HasLoggedErrors;
		}

		private static string NormalizePath(string path) {
			return Path.GetFullPath(Regex.Replace(path, @"\\+", @"\"));
		}
	}
}
