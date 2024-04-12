using System;
using System.Diagnostics;
using System.IO;

namespace TSBDashboard.Services
{
	public class CrystalReportsViewerService
	{
		private string _filePath { get; set; }
		private string _programPath { get; set; }

		public CrystalReportsViewerService(string filePath)
		{
			_filePath = filePath;
			_programPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "CrystalReportsViewer_02.exe");
		}

		public void ExecuteProgram()
		{
			string arguments = $"\"{_filePath}\"";

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = _programPath,
				Arguments = arguments
			};

			try
			{
				using (Process process = Process.Start(startInfo))
				{
					process.WaitForExit();
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

	}
}
