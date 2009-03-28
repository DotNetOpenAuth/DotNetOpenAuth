//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenIdOfflineProvider {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Data;
	using System.Linq;
	using System.Windows;
	using log4net.Core;
	using log4net;

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		internal static ILog Logger = log4net.LogManager.GetLogger(typeof(App));

		/// <summary>
		/// Initializes a new instance of the <see cref="App"/> class.
		/// </summary>
		public App() {
			log4net.Config.XmlConfigurator.Configure();
		}
	}
}
