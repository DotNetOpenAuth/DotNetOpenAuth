//-----------------------------------------------------------------------
// <copyright file="Publicize.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using Microsoft.Build.BuildEngine;
	using Microsoft.Build.Utilities;
	using Microsoft.Build.Framework;

	public class Publicize : ToolTask {
		[Required]
		public string MSBuildExtensionsPath { get; set; }

		[Required]
		public ITaskItem Assembly { get; set; }

		public bool DelaySign { get; set; }

		public string KeyFile { get; set; }

		public bool SkipUnchangedFiles { get; set; }

		[Output]
		public ITaskItem AccessorAssembly { get; set; }

		/// <summary>
		/// Generates the full path to tool.
		/// </summary>
		/// <returns>An absolute path.</returns>
		protected override string GenerateFullPathToTool() {
			string toolPath = Path.Combine(this.MSBuildExtensionsPath, @"Microsoft\VisualStudio\v9.0\TeamTest\Publicize.exe");
			return toolPath;
		}

		/// <summary>
		/// Gets the name of the tool.
		/// </summary>
		/// <value>The name of the tool.</value>
		protected override string ToolName {
			get { return "Publicize.exe"; }
		}

		/// <summary>
		/// Validates the parameters.
		/// </summary>
		protected override bool ValidateParameters() {
			if (!base.ValidateParameters()) {
				return false;
			}

			if (this.DelaySign && string.IsNullOrEmpty(this.KeyFile)) {
				this.Log.LogError("DelaySign=true, but no KeyFile given.");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Generates the command line commands.
		/// </summary>
		protected override string GenerateCommandLineCommands() {
			CommandLineBuilder builder = new CommandLineBuilder();

			if (this.DelaySign) {
				builder.AppendSwitch("/delaysign");
			}

			builder.AppendSwitchIfNotNull("/keyfile:", this.KeyFile);

			builder.AppendFileNameIfNotNull(this.Assembly);

			return builder.ToString();
		}

		public override bool Execute() {
			this.AccessorAssembly = new TaskItem(this.Assembly);
			this.AccessorAssembly.ItemSpec = Path.Combine(
				Path.GetDirectoryName(this.AccessorAssembly.ItemSpec),
				Path.GetFileNameWithoutExtension(this.AccessorAssembly.ItemSpec) + "_Accessor") + Path.GetExtension(this.AccessorAssembly.ItemSpec);

			if (this.SkipUnchangedFiles && File.GetLastWriteTimeUtc(this.Assembly.ItemSpec) < File.GetLastWriteTimeUtc(this.AccessorAssembly.ItemSpec)) {
				Log.LogMessage(MessageImportance.Low, "Skipping public accessor generation for {0} because {1} is up to date.", this.Assembly.ItemSpec, this.AccessorAssembly.ItemSpec);
				return true;
			}

			return base.Execute();
		}
	}
}
