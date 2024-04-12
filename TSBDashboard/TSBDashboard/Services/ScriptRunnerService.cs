using System;
using System.Diagnostics;

namespace TSBDashboard.Services
{
	public class ScriptRunnerService
	{
		private string FilePath { get; set; }

		public ScriptRunnerService(string filePath)
		{
			FilePath = filePath;
		}

		public void ExecuteProgram()
		{
			string scriptRunnerFilePath = "C:\\Program Files (x86)\\ISTC\\SQL Script Runner\\SQL_Script_Runner.exe";

			try
			{
				Process.Start(scriptRunnerFilePath, FilePath);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}
