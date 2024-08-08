using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Windows.Forms;

using System.Windows;

namespace TSBDashboard.Views
{
	/// <summary>
	/// Interaction logic for CrystalReportViewerWindow.xaml
	/// </summary>
	public partial class CrystalReportViewerWindow : Window
	{
		public CrystalReportViewerWindow(string reportPath)
		{
			InitializeComponent();
			LoadReport(reportPath);
		}

		private void LoadReport(string reportPath)
		{
			var crystalReportViewer = new CrystalReportViewer();
			ReportDocument reportDocument = new ReportDocument();
			reportDocument.Load(reportPath);
			crystalReportViewer.ReportSource = reportDocument;
			windowsFormsHost.Child = crystalReportViewer;
		}
	}
}
