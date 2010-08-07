using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace DotNetOpenAuth.BuildTasks {
	public class SetEnvironmentVariable : Task {
		public SetEnvironmentVariable() {
			Scope = EnvironmentVariableTarget.Process;
		}

		/// <summary>
		/// The name of the environment variable to set or clear.
		/// </summary>
		[Required]
		public string Name { get; set; }
		/// <summary>
		/// The value of the environment variable, or the empty string to clear it.
		/// </summary>
		[Required]
		public string Value { get; set; }
		/// <summary>
		/// The target environment for the variable.  Machine, User, or Process.
		/// </summary>
		public EnvironmentVariableTarget Scope { get; set; }

		public override bool Execute() {
			Environment.SetEnvironmentVariable(Name, Value, Scope);
			return true;
		}
	}
}
