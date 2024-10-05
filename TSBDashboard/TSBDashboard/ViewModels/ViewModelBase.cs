using System;
using System.ComponentModel;

namespace TSBDashboard.ViewModels
{
	public class ViewModelBase : INotifyPropertyChanged, IDisposable
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		// IDisposable implementation
		private bool _disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Providing a structure for the Dispose method to be overridden in derived classes.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// Dispose managed resources here
				}

				// Dispose unmanaged resources here

				_disposed = true;
			}
		}

		/// <summary>
		/// Finalizer for the ViewModelBase class.
		/// </summary>
		~ViewModelBase()
		{
			Dispose(false);
		}

		public ViewModelBase() { }
	}
}
