//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Runtime.InteropServices;
	using System.Windows;
	using System.Windows.Interop;

	/// <summary>
	/// P/Invoke methods and wrappers.
	/// </summary>
	internal static class NativeMethods {
		/// <summary>
		/// Gets the HWND of the current foreground window on the system.
		/// </summary>
		/// <returns>A handle to the foreground window.</returns>
		[DllImport("user32.dll")]
		internal static extern IntPtr GetForegroundWindow();

		/// <summary>
		/// Sets the foreground window of the system.
		/// </summary>
		/// <param name="hWnd">The HWND of the window to set as active.</param>
		/// <returns>A value indicating whether the foreground window was actually changed.</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetForegroundWindow(IntPtr hWnd);

		/// <summary>
		/// Sets the foreground window of the system.
		/// </summary>
		/// <param name="window">The window to bring to the foreground.</param>
		/// <returns>
		/// A value indicating whether the foreground window was actually changed.
		/// </returns>
		internal static bool SetForegroundWindow(Window window) {
			return SetForegroundWindow(new WindowInteropHelper(window).Handle);
		}

		/// <summary>
		/// Sets the active window of the process.
		/// </summary>
		/// <param name="window">The window to bring to the foreground.</param>
		internal static void SetActiveWindow(Window window) {
			SetActiveWindow(new WindowInteropHelper(window).Handle);
		}

		/// <summary>
		/// Sets the active window of the process.
		/// </summary>
		/// <param name="hWnd">The HWND of the window to set as active.</param>
		/// <returns>The window that was previously active?</returns>
		[DllImport("user32.dll")]
		private static extern IntPtr SetActiveWindow(IntPtr hWnd);
	}
}
