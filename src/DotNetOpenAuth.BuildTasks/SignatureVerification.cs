//-----------------------------------------------------------------------
// <copyright file="SignatureVerification.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.BuildTasks {
	using Microsoft.Build.Utilities;

	public class SignatureVerification : SnToolTask {
		/// <summary>
		/// Gets or sets a value indicating whether to register the given assembly and public key token
		/// for skip verification or clear any pre-existing skip verification entry.
		/// </summary>
		public bool SkipVerification { get; set; }

		/// <summary>
		/// Gets or sets the name of the assembly.
		/// </summary>
		/// <value>The name of the assembly.</value>
		public string AssemblyName { get; set; }

		/// <summary>
		/// Gets or sets the public key token.
		/// </summary>
		/// <value>The public key token.</value>
		public string PublicKeyToken { get; set; }

		/// <summary>
		/// Generates the command line commands.
		/// </summary>
		protected override string GenerateCommandLineCommands() {
			CommandLineBuilder builder = new CommandLineBuilder();
			builder.AppendSwitch("-q");
			if (this.SkipVerification) {
				builder.AppendSwitch("-Vr");
			} else {
				builder.AppendSwitch("-Vu");
			}

			builder.AppendFileNameIfNotNull(this.AssemblyName + "," + this.PublicKeyToken);
			return builder.ToString();
		}
	}
}
