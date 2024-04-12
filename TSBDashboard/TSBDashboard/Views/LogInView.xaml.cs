using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TSBDashboard.Views
{
	/// <summary>
	/// Interaction logic for LogInView.xaml
	/// </summary>
	public partial class LogInView : Window
	{
		public LogInView()
		{
			InitializeComponent();
		}

		private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private void Minimize_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void UserName_TextChanged(object sender, TextChangedEventArgs e)
		{
			CheckCredentials();
		}

		private void Password_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (DataContext != null)
			{
				var passwordBox = (PasswordBox)sender;
				((dynamic)DataContext).Password = passwordBox.SecurePassword;
				CheckCredentials();
			}
		}

		private void CheckCredentials()
		{
			if (!string.IsNullOrWhiteSpace(UserName.Text) && !string.IsNullOrWhiteSpace(Password.Password))
			{
				LoginButton.Visibility = Visibility.Visible;
			}
			else
			{
				LoginButton.Visibility = Visibility.Collapsed;
			}
		}


	}
}
