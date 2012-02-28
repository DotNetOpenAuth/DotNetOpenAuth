﻿namespace OAuthConsumer {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web;

	/// <summary>
	/// Logging tools for this sample.
	/// </summary>
	public static class Logging {
		/// <summary>
		/// An application memory cache of recent log messages.
		/// </summary>
		public static StringBuilder LogMessages = new StringBuilder();

		/// <summary>
		/// The logger for this sample to use.
		/// </summary>
		public static log4net.ILog Logger = log4net.LogManager.GetLogger("DotNetOpenAuth.OAuthConsumer");
	}
}