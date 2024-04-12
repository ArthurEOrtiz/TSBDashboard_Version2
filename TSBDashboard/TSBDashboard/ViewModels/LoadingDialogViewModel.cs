using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSBDashboard.ViewModels
{
	public class LoadingDialogViewModel : ViewModelBase
	{
		private readonly System.Timers.Timer _loadingTextTimer;

		private string _loadingText;
		public string LoadingText
		{
			get => _loadingText;
			set
			{
				_loadingText = value;
				OnPropertyChanged(nameof(LoadingText));
			}
		}

		public LoadingDialogViewModel()
		{
			LoadingText = "Loading";
			_loadingTextTimer = new System.Timers.Timer(500);
			_loadingTextTimer.Elapsed += OnLoadingTextTimerElapsed;
			_loadingTextTimer.Start();
		}

		private void OnLoadingTextTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			switch (LoadingText)
			{
				case "Loading":
					LoadingText = "Loading.";
					break;
				case "Loading.":
					LoadingText = "Loading..";
					break;
				case "Loading..":
					LoadingText = "Loading...";
					break;
				case "Loading...":
				default:
					LoadingText = "Loading";
					break;
			}
		}

		public void Dispose()
		{
			_loadingTextTimer.Stop();
			_loadingTextTimer.Dispose();
		}
	}
}
