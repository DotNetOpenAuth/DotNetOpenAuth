//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Runtime.InteropServices;
	using System.Windows;
	using System.Windows.Interop;

	internal static class NativeMethods {
		/// <summary>
		/// Gets the HWND of the current foreground window on the system.
		/// </summary>
		/// <returns>A handle to the foreground window.</returns>
		[DllImport("user32.dll")]
		internal static extern IntPtr GetForegroundWindow();
		
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern IntPtr SetActiveWindow(IntPtr hWnd);

		internal static bool SetForegroundWindow(Window window) {
			return SetForegroundWindow(new WindowInteropHelper(window).Handle);
		}

		internal static void SetActiveWindow(Window window) {
			SetActiveWindow(new WindowInteropHelper(window).Handle);
		}
	}
}
