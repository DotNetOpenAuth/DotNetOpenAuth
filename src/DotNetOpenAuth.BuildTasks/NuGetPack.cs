//-----------------------------------------------------------------------
// <copyright file="NuGetPack.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	/// <summary>
	/// Creates a .nupkg archive from a .nuspec file and content files.
	/// </summary>
	public class NuGetPack : ToolTask {
		/// <summary>
		/// Gets or sets the path to the .nuspec file.
		/// </summary>
		[Required]
		public ITaskItem NuSpec { get; set; }

		/// <summary>
		/// Gets or sets the base directory, the contents of which gets included in the .nupkg archive.
		/// </summary>
		public ITaskItem BaseDirectory { get; set; }

		/// <summary>
		/// Gets or sets the path to the directory that will contain the generated .nupkg archive.
		/// </summary>
		public ITaskItem OutputPackageDirectory { get; set; }

		/// <summary>
		/// Gets or sets the semicolon delimited list of properties to supply to the NuGet Pack command to perform token substitution.
		/// </summary>
		public string Properties { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate a symbols nuget package.
		/// </summary>
		public bool Symbols { get; set; }

		/// <summary>
		/// Returns the fully qualified path to the executable file.
		/// </summary>
		/// <returns>
		/// The fully qualified path to the executable file.
		/// </returns>
		protected override string GenerateFullPathToTool() {
			return this.ToolPath;
		}

		/// <summary>
		/// Gets the name of the executable file to run.
		/// </summary>
		/// <returns>The name of the executable file to run.</returns>
		protected override string ToolName {
			get { return "NuGet.exe"; }
		}

		/// <summary>
		/// Runs the exectuable file with the specified task parameters.
		/// </summary>
		/// <returns>
		/// true if the task runs successfully; otherwise, false.
		/// </returns>
		public override bool Execute() {
			if (this.OutputPackageDirectory != null && Path.GetDirectoryName(this.OutputPackageDirectory.ItemSpec).Length > 0) {
				Directory.CreateDirectory(Path.GetDirectoryName(this.OutputPackageDirectory.ItemSpec));
			}

			bool result = base.Execute();
			return result;
		}

		/// <summary>
		/// Returns a string value containing the command line arguments to pass directly to the executable file.
		/// </summary>
		/// <returns>
		/// A string value containing the command line arguments to pass directly to the executable file.
		/// </returns>
		protected override string GenerateCommandLineCommands() {
			var args = new CommandLineBuilder();

			args.AppendSwitch("pack");
			args.AppendFileNameIfNotNull(this.NuSpec);
			args.AppendSwitchIfNotNull("-BasePath ", this.BaseDirectory);
			args.AppendSwitchIfNotNull("-OutputDirectory ", this.OutputPackageDirectory);
			args.AppendSwitchIfNotNull("-Properties ", this.Properties);
			if (this.Symbols) {
				args.AppendSwitch("-Symbols");
			}

			return args.ToString();
		}
	}
}
