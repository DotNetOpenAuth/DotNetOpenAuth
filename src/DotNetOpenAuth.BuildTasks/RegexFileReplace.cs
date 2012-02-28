//-----------------------------------------------------------------------
// <copyright file="RegexFileReplace.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using Microsoft.Build.BuildEngine;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;
	using System.Text.RegularExpressions;

	public class RegexFileReplace : Task {
		/// <summary>
		/// Gets or sets the set of files to search.
		/// </summary>
		[Required]
		public ITaskItem[] Files { get; set; }

		/// <summary>
		/// Gets or sets the files to save the changed files to.  This may be the same as the input files to make the change in place.
		/// </summary>
		public ITaskItem[] OutputFiles { get; set; }

		public string Pattern { get; set; }

		public string Replacement { get; set; }

		/// <summary>
		/// Executes this instance.
		/// </summary>
		public override bool Execute() {
			if (this.OutputFiles == null || this.OutputFiles.Length == 0) {
				this.OutputFiles = this.Files;
			}

			foreach (var file in this.Files) {
				string[] lines = File.ReadAllLines(file.ItemSpec);
				for (int i = 0; i < lines.Length; i++) {
					lines[i] = Regex.Replace(lines[i], this.Pattern, this.Replacement);
				}

				File.WriteAllLines(file.ItemSpec, lines);
			}

			return !this.Log.HasLoggedErrors;
		}
	}
}
