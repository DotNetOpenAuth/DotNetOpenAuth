//-----------------------------------------------------------------------
// <copyright file="SnToolTask.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft SDKs\Windows\v6.0A\bin\" + this.ToolName);
		}
	}
}
