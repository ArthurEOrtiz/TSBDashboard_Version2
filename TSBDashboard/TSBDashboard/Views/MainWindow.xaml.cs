using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TSBDashboard.ViewModels;

namespace TSBDashboard
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
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
			Application.Current.Shutdown();
			this.Close();
		}

		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			// Get the search query
			string query = SearchBox.Text;

			MainWindowViewModel viewModel = DataContext as MainWindowViewModel;
			viewModel?.FilterDirectoryItems(query);

		}

		private void TreeView_Main_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (TreeView_Main.SelectedItem is DirectoryItemViewModel selectedItem && !selectedItem.IsDirectory)
			{
				MainWindowViewModel viewModel = DataContext as MainWindowViewModel;

				viewModel?.DownloadFileAsync(selectedItem);

			}
		}
	}
}
