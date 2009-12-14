namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.Build.Utilities;
	using Microsoft.Build.Framework;

	public class FilterItems : Task {
		[Required]
		public ITaskItem[] InputItems { get; set; }

		[Required]
		public ITaskItem[] StartsWithAny { get; set; }

		[Output]
		public ITaskItem[] FilteredItems { get; set; }

		public override bool Execute() {
			FilteredItems = InputItems.Where(item => StartsWithAny.Any(filter => item.ItemSpec.StartsWith(filter.ItemSpec, StringComparison.OrdinalIgnoreCase))).ToArray();
			return true;
		}
	}
}
