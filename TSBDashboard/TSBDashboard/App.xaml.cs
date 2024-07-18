using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;
using TSBDashboard.Options;
using TSBDashboard.Services;
using TSBDashboard.ViewModels;
using TSBDashboard.Views;

namespace TSBDashboard
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private readonly ServiceProvider _serviceProvider;
		public static event Action ApplicationExiting;

		public App()
		{
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);
			_serviceProvider = serviceCollection.BuildServiceProvider();
		}

		// Configuration
		/// <summary>
		/// This method is used to set up Dependency Injection (DI) for the application. 
		/// Dependency Injection is a design pattern that helps to reduce the dependency 
		/// of one class on another class. It is a way to achieve Inversion of Control (IoC).
		/// </summary>
		/// <param name="services"><see cref="IServiceCollection"/> is an interface provided by the .NET Core framework. It represents a collection of service descriptors, which are essentially the blueprints for creating service instances.</param>
		private void ConfigureServices(IServiceCollection services)
		{
			IConfiguration configuration;

			try
			{
				configuration = new ConfigurationBuilder()
					.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: false, reloadOnChange: true)
					.Build();
			}
			catch
			{
				Environment.Exit(1);
				return;
			}

			services.AddSingleton<IConfiguration>(configuration);
			services.Configure<SftpSettings>(configuration.GetSection("SftpSettings"));
			services.AddSingleton<SftpService>(); // Singleton so that we don't make multiple connections to the SFTP server.
			services.AddTransient<LogInViewModel>();
			services.AddTransient<MainWindowViewModel>();
		}

		// Overrides
		/// <summary>
		/// This method is called when the application starts up. It is overridden to perform initialization actions before the main window is shown.
		/// 
		/// The method does the following:
		/// <para>1. Calls the base class's OnStartup method to ensure any base class startup actions are performed.</para>
		/// <para>2. Calls the InitializeLocalAppDataFolder method to set up a local folder for the application to store its data.</para>
		/// <para>3. Retrieves the LogInViewModel from the service provider. This ViewModel is used to handle the logic for the login view.</para>
		/// <para>4. Subscribes to the SuccessfulLogin event of the LogInViewModel. This event is raised when a login is successful.</para>
		/// <para>5. Creates a new instance of the LogInView window and sets its DataContext to the LogInViewModel. This binds the ViewModel to the View, allowing them to communicate.</para>
		/// <para>6. Sets the startup location of the LogInView window to the center of the screen.</para>
		/// <para>7. Shows the LogInView window, making it visible to the user.</para>
		/// </summary>
		/// <param name="e">Contains data about the startup event.</param>
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			InitializeLocalAppDataFolder();

			var logInViewModel = _serviceProvider.GetService<LogInViewModel>();
			logInViewModel.SuccessfulLogin += LogInViewModel_SuccessfulLogin;
			Window logInView = new LogInView
			{
				DataContext = logInViewModel,
				WindowStartupLocation = WindowStartupLocation.CenterScreen
			};
			logInView.Show();
		}

		/// <summary>
		/// This method is called when the application is exiting. It is overridden to perform cleanup actions before the application closes.
		/// 
		/// The method does the following:
		/// <para>1. Raises the ApplicationExiting event. This event is used to signal other parts of the application that the application is exiting. For example, the SftpService subscribes to this event and closes the SFTP session when the event is raised.</para>
		/// <para>2. Calls the base class's OnExit method to ensure any base class cleanup actions are performed.</para>
		/// </summary>
		/// <param name="e">Contains data about the exit event.</param>
		protected override void OnExit(ExitEventArgs e)
		{
			ApplicationExiting?.Invoke();
			DeleteAppData();
			base.OnExit(e);
		}

		// Helper Methods
		/// <summary>
		/// Handles the SuccessfulLogin event from the LogInViewModel. This method is called when a user 
		/// successfully logs in. It is currently used to transition to the main application window, but 
		/// could also be used to perform user-specific initialization, or trigger other actions that 
		/// should occur after a successful login.
		/// </summary>
		private void LogInViewModel_SuccessfulLogin()
		{
			var mainWindowViewModel = _serviceProvider.GetService<MainWindowViewModel>();
			Window mainWindow = new MainWindow { DataContext = mainWindowViewModel, };
			
			var currentWindow = Current.MainWindow;
			mainWindow.Owner = Current.MainWindow;

			mainWindow.Left = currentWindow.Left + ((currentWindow.Width - mainWindow.Width) / 2);
			mainWindow.Top = currentWindow.Top + ((currentWindow.Height - mainWindow.Height) / 2);

			mainWindow.Show();
			mainWindow.Owner = null;

			Current.MainWindow = mainWindow;

			currentWindow.Close();
		}

		/// <summary>
		/// Initializes the local application data folder.
		/// This method is responsible for setting up the local storage used by the application.
		/// It typically checks if a specific directory exists in the user's local AppData folder.
		/// If the directory does not exist, the method creates it.
		/// This directory can be used to store application data such as configuration files, 
		/// temporary files, or cached data.
		/// </summary>
		private void InitializeLocalAppDataFolder()
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string myAppDataPath = Path.Combine(appDataPath, "TSBDashboard");

			if (!Directory.Exists(myAppDataPath))
			{
				Directory.CreateDirectory(myAppDataPath);
			}
		}

		/// <summary>
		/// Deletes the app data folder, if it exists. 
		/// </summary>
		private void DeleteAppData()
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string myAppDataPath = Path.Combine(appDataPath, "TSBDashboard");

			if (Directory.Exists(myAppDataPath))
			{
				Directory.Delete(myAppDataPath, true);
			}
		}

	}
}
