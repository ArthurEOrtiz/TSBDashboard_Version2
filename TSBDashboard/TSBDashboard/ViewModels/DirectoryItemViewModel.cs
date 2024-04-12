using System.Collections.Generic;
using System.Windows;

namespace TSBDashboard.ViewModels
{
	public class DirectoryItemViewModel : ViewModelBase
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public bool IsDirectory { get; set; }

		private bool _isExpanded;
		public bool IsExpanded
		{
			get => _isExpanded;
			set
			{
				_isExpanded = value;
				OnPropertyChanged(nameof(IsExpanded));
			}
		}

		private Visibility _visibility;
		public Visibility Visibility
		{
			get => _visibility;
			set
			{
				if (_visibility != value)
				{
					_visibility = value; ;
					OnPropertyChanged(nameof(Visibility));
				}
			}
		}
		public List<DirectoryItemViewModel> SubItems { get; set; } = new List<DirectoryItemViewModel>();

		public DirectoryItemViewModel() { }
	}
}
