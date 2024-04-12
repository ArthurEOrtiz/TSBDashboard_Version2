using System;
using System.Security;
using System.Windows.Input;
using TSBDashboard.Commands;
using TSBDashboard.Exceptions;
using TSBDashboard.Services;

namespace TSBDashboard.ViewModels
{
	public class LogInViewModel : ViewModelBase
	{
		public event Action SuccessfulLogin;

		private string _userName;
		public string UserName
		{
			get => _userName;
			set
			{
				_userName = value;
				OnPropertyChanged(nameof(UserName));
				((RelayCommand)LogInCommand).RaiseCanExecuteChanged();
			}
		}

		private SecureString _password;
		public SecureString Password
		{
			get => _password;
			set
			{
				_password = value;
				OnPropertyChanged(nameof(Password));
				((RelayCommand)LogInCommand).RaiseCanExecuteChanged();
			}
		}

		private string _errorMessage;
		public string ErrorMessage
		{
			get => _errorMessage;
			set
			{
				_errorMessage = value;
				OnPropertyChanged(nameof(ErrorMessage));
			}
		}

		public ICommand LogInCommand { get; private set; }
		private readonly SftpService _sftpService;

		public LogInViewModel(SftpService sftpService)
		{
			_sftpService = sftpService;
			LogInCommand = new RelayCommand(LogIn, CanLogIn);
		}

		private void LogIn(object parameter)
		{
			try
			{
				_sftpService.LogIn(UserName, Password);
				SuccessfulLogin?.Invoke();
			}
			catch (AuthenticationFailedException ex)
			{
				ErrorMessage = ex.Message;
			}
			catch (SftpServiceException ex)
			{
				ErrorMessage = ex.Message;
			}
		}



		private bool CanLogIn(object parameter)
		{
			return !string.IsNullOrEmpty(UserName) && Password != null && Password.Length > 0;
		}

	}
}