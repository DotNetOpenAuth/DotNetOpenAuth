//-----------------------------------------------------------------------
// <copyright file="SnToolTask.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.IO;
	using Microsoft.Build.Utilities;

	public abstract class SnToolTask : ToolTask {
		/// <summary>
		/// Gets the name of the tool.
		/// </summary>
		/// <value>The name of the tool.</value>
		protected override string ToolName {
			get { return "sn.exe"; }
		}

		/// <summary>
		/// Generates the full path to tool.
		/// </summary>
		protected override string GenerateFullPathToTool() {
			string[] versions = new[] { "v6.0A", "v6.1", "v7.0a" };
			string fullPath = null;
			foreach (string version in versions) {
				fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft SDKs\Windows\" + version + @"\bin\" + this.ToolName);
				if (File.Exists(fullPath)) {
					return fullPath;
				}
			}

			throw new FileNotFoundException("Unable to find sn.exe tool.", fullPath);
		}
	}
}
