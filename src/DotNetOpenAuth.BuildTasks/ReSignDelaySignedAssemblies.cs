//-----------------------------------------------------------------------
// <copyright file="ReSignDelaySignedAssemblies.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using System;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	public class ReSignDelaySignedAssemblies : SnToolTask {
		/// <summary>
		/// Gets or sets the key file to use for signing.
		/// </summary>
		public ITaskItem KeyFile { get; set; }

		/// <summary>
		/// Gets or sets the key container.
		/// </summary>
		public ITaskItem KeyContainer { get; set; }

		/// <summary>
		/// Gets or sets the assemblies to re-sign.
		/// </summary>
		public ITaskItem[] Assemblies { get; set; }

		/// <summary>
		/// Generates the command line commands.
		/// </summary>
		protected override string GenerateCommandLineCommands() {
			////if (this.Assemblies.Length != 1) {
			////    throw new NotSupportedException("Exactly 1 assembly for signing is supported.");
			////}
			var args = new CommandLineBuilder();
			args.AppendSwitch("-q");

			if (this.KeyFile != null) {
				args.AppendSwitch("-R");
			} else if (this.KeyContainer != null) {
				args.AppendSwitch("-Rc");
			} else {
				throw new InvalidOperationException("Either KeyFile or KeyContainer must be set.");
			}

			args.AppendFileNameIfNotNull(this.Assemblies[0]);
			if (this.KeyFile != null) {
				args.AppendFileNameIfNotNull(this.KeyFile);
			} else {
				args.AppendFileNameIfNotNull(this.KeyContainer);
			}

			return args.ToString();
		}
	}
}
