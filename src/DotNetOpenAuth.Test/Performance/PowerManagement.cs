//-----------------------------------------------------------------------
// <copyright file="PowerManagement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Performance {
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Runtime.InteropServices;

	/// <summary>
	/// PowerManagement allows you to access the funtionality of the Control Panel -> Power Options
	/// dialog in windows.  (Currently we only use VISTA APIs). 
	/// </summary>
	internal static class PowerManagment {
		internal static unsafe Guid CurrentPolicy {
			get {
				Guid* retPolicy = null;
				Guid ret = Guid.Empty;
				try {
					int callRet = PowerGetActiveScheme(IntPtr.Zero, ref retPolicy);
					if (callRet == 0) {
						ret = *retPolicy;
						Marshal.FreeHGlobal((IntPtr)retPolicy);
					}
				} catch (Exception) {
				}
				return ret;
			}

			set {
				Guid newPolicy = value;
				int result = PowerSetActiveScheme(IntPtr.Zero, ref newPolicy);
				if (result != 0) {
					throw new Win32Exception(result);
				}
			}
		}

		[DllImport("powrprof.dll")]
		private static unsafe extern int PowerGetActiveScheme(IntPtr reservedZero, ref Guid* policyGuidRet);

		[DllImport("powrprof.dll")]
		private static extern int PowerSetActiveScheme(IntPtr reservedZero, ref Guid policyGuid);

		internal static class PowerProfiles {
			internal static Guid HighPerformance = new Guid(0x8c5e7fda, 0xe8bf, 0x4a96, 0x9a, 0x85, 0xa6, 0xe2, 0x3a, 0x8c, 0x63, 0x5c);

			internal static Guid Balanced = new Guid(0x381b4222, 0xf694, 0x41f0, 0x96, 0x85, 0xff, 0x5b, 0xb2, 0x60, 0xdf, 0x2e);

			internal static Guid PowerSaver = new Guid(0xa1841308, 0x3541, 0x4fab, 0xbc, 0x81, 0xf7, 0x15, 0x56, 0xf2, 0x0b, 0x4a);
		}

		internal class PowerSetting : IDisposable {
			/// <summary>
			/// The power policy in effect when this instance was constructed.
			/// </summary>
			private Guid previousPolicy;

			/// <summary>
			/// Initializes a new instance of the <see cref="PowerSetting"/> class.
			/// </summary>
			/// <param name="powerProfile">The power profile.</param>
			internal PowerSetting(Guid powerProfile) {
				this.previousPolicy = PowerManagment.CurrentPolicy;
				if (this.previousPolicy != powerProfile) {
					PowerManagment.CurrentPolicy = powerProfile;
				}
			}

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose() {
				if (this.previousPolicy != PowerManagment.CurrentPolicy) {
					PowerManagment.CurrentPolicy = this.previousPolicy;
				}
			}
		}
	}
}