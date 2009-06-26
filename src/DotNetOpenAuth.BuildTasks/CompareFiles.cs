using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

namespace DotNetOpenAuth.BuildTasks {
	public class CompareFiles : Task {
		/// <summary>
		/// One set of items to compare.
		/// </summary>
		[Required]
		public ITaskItem[] OriginalItems { get; set; }

		/// <summary>
		/// The other set of items to compare.
		/// </summary>
		[Required]
		public ITaskItem[] NewItems { get; set; }

		/// <summary>
		/// Gets whether the items lists contain items that are identical going down the list.
		/// </summary>
		[Output]
		public bool AreSame { get; private set; }

		/// <summary>
		/// Same as <see cref="AreSame"/>, but opposite.
		/// </summary>
		[Output]
		public bool AreChanged { get { return !AreSame; } }

		public override bool Execute() {
			AreSame = AreFilesIdentical();
			return true;
		}

		private bool AreFilesIdentical() {
			if (OriginalItems.Length != NewItems.Length) {
				return false;
			}

			for (int i = 0; i < OriginalItems.Length; i++) {
				if (!IsContentOfFilesTheSame(OriginalItems[i].ItemSpec, NewItems[i].ItemSpec)) {
					return false;
				}
			}

			return true;
		}

		private bool IsContentOfFilesTheSame(string file1, string file2) {
			// If exactly one file is missing, that's different.
			if (File.Exists(file1) ^ File.Exists(file2)) return false;
			// If both are missing, that's the same.
			if (!File.Exists(file1)) return true;
			// If both are present, we need to do a content comparison.
			using (FileStream fileStream1 = File.OpenRead(file1)) {
				using (FileStream fileStream2 = File.OpenRead(file2)) {
					if (fileStream1.Length != fileStream2.Length) return false;
					byte[] buffer1 = new byte[4096];
					byte[] buffer2 = new byte[buffer1.Length];
					int bytesRead;
					do {
						bytesRead = fileStream1.Read(buffer1, 0, buffer1.Length);
						if (fileStream2.Read(buffer2, 0, buffer2.Length) != bytesRead) {
							// We should never get here since we compared file lengths, but
							// this is a sanity check.
							return false;
						}
						for (int i = 0; i < bytesRead; i++) {
							if (buffer1[i] != buffer2[i]) {
								return false;
							}
						}
					} while (bytesRead == buffer1.Length);
				}
			}

			return true;
		}
	}
}
