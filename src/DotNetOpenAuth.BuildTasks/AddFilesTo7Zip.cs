//-----------------------------------------------------------------------
// <copyright file="AddFilesTo7Zip.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.Build.Utilities;
	using Microsoft.Build.Framework;

	public class AddFilesTo7Zip : ToolTask {
		/// <summary>
		/// Initializes a new instance of the <see cref="AddFilesTo7Zip"/> class.
		/// </summary>
		public AddFilesTo7Zip() {
			this.YieldDuringToolExecution = true;
		}

		[Required]
		public ITaskItem ZipFileName { get; set; }

		[Required]
		public ITaskItem[] Files { get; set; }

		public string WorkingDirectory { get; set; }

		/// <summary>
		/// Gets the name of the tool.
		/// </summary>
		/// <value>
		/// The name of the tool.
		/// </value>
		protected override string ToolName {
			get { return "7za.exe"; }
		}

		/// <summary>
		/// Generates the full path to tool.
		/// </summary>
		protected override string GenerateFullPathToTool() {
			return this.ToolPath;
		}

		protected override string GenerateCommandLineCommands() {
			var args = new CommandLineBuilder();

			args.AppendSwitch("a");
			args.AppendSwitch("--");

			args.AppendFileNameIfNotNull(this.ZipFileName);

			return args.ToString();
		}

		/// <summary>
		/// Gets the response file switch.
		/// </summary>
		/// <param name="responseFilePath">The response file path.</param>
		protected override string GetResponseFileSwitch(string responseFilePath) {
			return "@" + responseFilePath;
		}

		/// <summary>
		/// Gets the response file encoding.
		/// </summary>
		/// <value>
		/// The response file encoding.
		/// </value>
		protected override Encoding ResponseFileEncoding {
			get { return Encoding.UTF8; }
		}

		/// <summary>
		/// Generates the response file commands.
		/// </summary>
		protected override string GenerateResponseFileCommands() {
			var args = new CommandLineBuilder();
			args.AppendFileNamesIfNotNull(this.Files.Select(GetWorkingDirectoryRelativePath).ToArray(), Environment.NewLine);
			return args.ToString();
		}

		/// <summary>
		/// Gets the working directory.
		/// </summary>
		protected override string GetWorkingDirectory() {
			if (!String.IsNullOrEmpty(this.WorkingDirectory)) {
				return this.WorkingDirectory;
			} else {
				return base.GetWorkingDirectory();
			}
		}

		private string GetWorkingDirectoryRelativePath(ITaskItem taskItem) {
			if (taskItem.ItemSpec.StartsWith(this.WorkingDirectory, StringComparison.OrdinalIgnoreCase)) {
				return taskItem.ItemSpec.Substring(this.WorkingDirectory.Length);
			} else {
				return taskItem.ItemSpec;
			}
		}
	}
}
