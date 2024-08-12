using System;
using System.Windows;
using TSBDashboard.Views;

namespace TSBDashboard.Services
{
	public class CrystalReportsViewerService
	{
		private string _filePath { get; set; }

		public CrystalReportsViewerService(string filePath)
		{
			_filePath = filePath;
		}

		public void ShowReport()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				var viewerWindow = new CrystalReportViewerWindow(_filePath);
				viewerWindow.Show();
			});
		}
	}
}