namespace YOURLIBNAME.Loggers {
	using System;
	using System.Diagnostics;
	using System.Security;
	using System.Security.Permissions;

	class TraceLogger : ILog {
		TraceSwitch traceSwitch = new TraceSwitch("OpenID", "OpenID Trace Switch");

		static bool IsSufficientPermissionGranted {
			get {
				PermissionSet permissions = new PermissionSet(PermissionState.None);
				permissions.AddPermission(new KeyContainerPermission(PermissionState.Unrestricted));
				permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
				permissions.AddPermission(new RegistryPermission(PermissionState.Unrestricted));
				permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.UnmanagedCode | SecurityPermissionFlag.ControlThread));
				var file = new FileIOPermission(PermissionState.None);
				file.AllFiles = FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read;
				permissions.AddPermission(file);
				try {
					permissions.Demand();
					return true;
				} catch (SecurityException) {
					return false;
				}
			}
		}

		#region ILog Members

		public bool IsDebugEnabled {
			get { return traceSwitch.TraceVerbose; }
		}

		public bool IsInfoEnabled {
			get { return traceSwitch.TraceInfo; }
		}

		public bool IsWarnEnabled {
			get { return traceSwitch.TraceWarning; }
		}

		public bool IsErrorEnabled {
			get { return traceSwitch.TraceError; }
		}

		public bool IsFatalEnabled {
			get { return traceSwitch.TraceError; }
		}

		public void Debug(object message) {
			Trace.TraceInformation(message.ToString());
		}

		public void Debug(object message, Exception exception) {
			Trace.TraceInformation(message + ": " + exception.ToString());
		}

		public void DebugFormat(string format, params object[] args) {
			Trace.TraceInformation(format, args);
		}

		public void DebugFormat(string format, object arg0) {
			Trace.TraceInformation(format, arg0);
		}

		public void DebugFormat(string format, object arg0, object arg1) {
			Trace.TraceInformation(format, arg0, arg1);
		}

		public void DebugFormat(string format, object arg0, object arg1, object arg2) {
			Trace.TraceInformation(format, arg0, arg1, arg2);
		}

		public void DebugFormat(IFormatProvider provider, string format, params object[] args) {
			Trace.TraceInformation(format, args);
		}

		public void Info(object message) {
			Trace.TraceInformation(message.ToString());
		}

		public void Info(object message, Exception exception) {
			Trace.TraceInformation(message + ": " + exception.ToString());
		}

		public void InfoFormat(string format, params object[] args) {
			Trace.TraceInformation(format, args);
		}

		public void InfoFormat(string format, object arg0) {
			Trace.TraceInformation(format, arg0);
		}

		public void InfoFormat(string format, object arg0, object arg1) {
			Trace.TraceInformation(format, arg0, arg1);
		}

		public void InfoFormat(string format, object arg0, object arg1, object arg2) {
			Trace.TraceInformation(format, arg0, arg1, arg2);
		}

		public void InfoFormat(IFormatProvider provider, string format, params object[] args) {
			Trace.TraceInformation(format, args);
		}

		public void Warn(object message) {
			Trace.TraceWarning(message.ToString());
		}

		public void Warn(object message, Exception exception) {
			Trace.TraceWarning(message + ": " + exception.ToString());
		}

		public void WarnFormat(string format, params object[] args) {
			Trace.TraceWarning(format, args);
		}

		public void WarnFormat(string format, object arg0) {
			Trace.TraceWarning(format, arg0);
		}

		public void WarnFormat(string format, object arg0, object arg1) {
			Trace.TraceWarning(format, arg0, arg1);
		}

		public void WarnFormat(string format, object arg0, object arg1, object arg2) {
			Trace.TraceWarning(format, arg0, arg1, arg2);
		}

		public void WarnFormat(IFormatProvider provider, string format, params object[] args) {
			Trace.TraceWarning(format, args);
		}

		public void Error(object message) {
			Trace.TraceError(message.ToString());
		}

		public void Error(object message, Exception exception) {
			Trace.TraceError(message + ": " + exception.ToString());
		}

		public void ErrorFormat(string format, params object[] args) {
			Trace.TraceError(format, args);
		}

		public void ErrorFormat(string format, object arg0) {
			Trace.TraceError(format, arg0);
		}

		public void ErrorFormat(string format, object arg0, object arg1) {
			Trace.TraceError(format, arg0, arg1);
		}

		public void ErrorFormat(string format, object arg0, object arg1, object arg2) {
			Trace.TraceError(format, arg0, arg1, arg2);
		}

		public void ErrorFormat(IFormatProvider provider, string format, params object[] args) {
			Trace.TraceError(format, args);
		}

		public void Fatal(object message) {
			Trace.TraceError(message.ToString());
		}

		public void Fatal(object message, Exception exception) {
			Trace.TraceError(message + ": " + exception.ToString());
		}

		public void FatalFormat(string format, params object[] args) {
			Trace.TraceError(format, args);
		}

		public void FatalFormat(string format, object arg0) {
			Trace.TraceError(format, arg0);
		}

		public void FatalFormat(string format, object arg0, object arg1) {
			Trace.TraceError(format, arg0, arg1);
		}

		public void FatalFormat(string format, object arg0, object arg1, object arg2) {
			Trace.TraceError(format, arg0, arg1, arg2);
		}

		public void FatalFormat(IFormatProvider provider, string format, params object[] args) {
			Trace.TraceError(format, args);
		}

		#endregion

		/// <summary>
		/// Returns a new logger that uses the <see cref="System.Diagnostics.Trace"/> class 
		/// if sufficient CAS permissions are granted to use it, otherwise returns false.
		/// </summary>
		/// <returns>The created <see cref="ILog"/> instance.</returns>
		internal static ILog Initialize() {
			return IsSufficientPermissionGranted ? new TraceLogger() : null;
		}
	}
}
