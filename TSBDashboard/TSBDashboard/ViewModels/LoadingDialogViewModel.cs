using System.Timers;

namespace TSBDashboard.ViewModels
{
	public class LoadingDialogViewModel : ViewModelBase
	{
		private readonly Timer _loadingTextTimer;

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
			_loadingTextTimer = new Timer(500);
			_loadingTextTimer.Elapsed += OnLoadingTextTimerElapsed;
			_loadingTextTimer.Start();
		}

		private void OnLoadingTextTimerElapsed(object sender, ElapsedEventArgs e)
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

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_loadingTextTimer.Stop();
				_loadingTextTimer.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
