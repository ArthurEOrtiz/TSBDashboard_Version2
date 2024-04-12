using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TSBDashboard.ViewModels;

namespace TSBDashboard.Views
{
	/// <summary>
	/// Interaction logic for LoadingDialogView.xaml
	/// </summary>
	public partial class LoadingDialogView : Window
	{
		public LoadingDialogView()
		{
			InitializeComponent();
			DataContext = new LoadingDialogViewModel();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			LoadingDialogViewModel viewModel = (LoadingDialogViewModel)DataContext;
			viewModel?.Dispose();
		}
	}
}
