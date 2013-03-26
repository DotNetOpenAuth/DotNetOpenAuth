namespace DotNetOpenAuth.Samples.OAuthConsumerWpf {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using Validation;

	/// <summary>
	/// Interaction logic for Authorize2.xaml
	/// </summary>
	public partial class Authorize2 : Window {
		internal Authorize2(UserAgentClient client) {
			Requires.NotNull(client, "client");

			this.InitializeComponent();
			this.clientAuthorizationView.Client = client;
		}

		public IAuthorizationState Authorization {
			get { return this.clientAuthorizationView.Authorization; }
		}

		public ClientAuthorizationView ClientAuthorizationView {
			get { return this.clientAuthorizationView; }
		}

		private void clientAuthorizationView_Completed(object sender, ClientAuthorizationCompleteEventArgs e) {
			this.DialogResult = e.Authorization != null && e.Authorization.AccessToken != null;
			this.Close();
		}
	}
}