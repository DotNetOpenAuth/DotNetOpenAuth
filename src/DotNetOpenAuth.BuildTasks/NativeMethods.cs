namespace DotNetOpenAuth.BuildTasks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Runtime.InteropServices;

	internal static class NativeMethods {
		[DllImport("kernel32", SetLastError = true)]
		private static extern bool CreateHardLink(string newFileName, string existingFileName, IntPtr securityAttributes);

		internal static void CreateHardLink(string existingFileName, string newFileName) {
			if (!CreateHardLink(newFileName, existingFileName, IntPtr.Zero)) {
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}
	}
}
