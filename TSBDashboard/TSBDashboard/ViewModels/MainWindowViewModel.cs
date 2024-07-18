using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using TSBDashboard.Services;
using TSBDashboard.Views;

namespace TSBDashboard.ViewModels
{
	/// <summary>
	/// ViewModel for the MainWindow. The View model is responsible for the following . . . 
	/// <para>Data Management: The ViewModel holds and manages the data that is to be displayed 
	/// in the View. It exposes data from the Model in a way that is easier for the View to use.</para>
	/// <para>Command Handling: The ViewModel defines commands that the View can bind to. These 
	/// commands encapsulate the logic for the actions that can be performed in the View.</para>
	/// <para>State Management: The ViewModel maintains the state of the View and provides 
	/// methods to change the state.</para>
	/// <para>Validation: The ViewModel can perform validation and business rule enforcement.</para>
	/// <para>Communication: The ViewModel communicates with other parts of the application, such 
	/// as services or repositories, to load and save data.</para>
	/// <para>Decoupling Logic from View: The ViewModel allows the View to be as simple as possible 
	/// by taking on responsibilities such as complex logic and computations.</para>
	/// </summary>
	public class MainWindowViewModel : ViewModelBase
	{
		private readonly SftpService _sftpService;

		private List<DirectoryItemViewModel> _directoryItemViewModel;
		public List<DirectoryItemViewModel> DirectoryItemViewModel
		{
			get => _directoryItemViewModel;
			set
			{
				_directoryItemViewModel = value;
				OnPropertyChanged(nameof(DirectoryItemViewModel));
			}
		}

		public MainWindowViewModel(SftpService sftpService)
		{
			_sftpService = sftpService;
			_ = LoadDataAsync();
		}

		/// <summary>
		/// Asynchronously loads data from the root directory ("/") and displays it in the 
		/// DirectoryItemViewModel property.This method first shows a loading dialog, then 
		/// retrieves the data, and finally closes the loading dialog.If an error occurs 
		/// during the retrieval of the data, an error dialog is shown with the error message.
		/// </summary>
		private async Task LoadDataAsync()
		{
			var loadingDialog = ShowLoadingDialog();

			try
			{
				DirectoryItemViewModel = await _sftpService.GetAllDirectoriesAndFiles("/");
			}
			catch (Exception ex)
			{
				loadingDialog.Close();
				ShowErrorDialog(ex.Message);
			}
			finally
			{
				loadingDialog.Close();
			}
		}

		/// <summary>
		/// Asynchronously downloads a file specified by the DirectoryItemViewModel parameter. 
		/// This method first shows a loading dialog, then initiates the file download, 
		/// and finally closes the loading dialog. If an error occurs during the download, 
		/// an error dialog is shown with the error message.
		/// </summary>
		/// <param name="item">The DirectoryItemViewModel representing the file to be downloaded.</param>
		public async Task DownloadFileAsync(DirectoryItemViewModel item)
		{
			var loadingDialog = ShowLoadingDialog();
			try
			{
				await _sftpService.DownloadFileAsync(item.Name, item.Path);
			}
			catch (Exception ex)
			{
				loadingDialog.Close();
				ShowErrorDialog(ex.Message);
			}
			finally
			{
				loadingDialog.Close();
			}
		}

		/// <summary>
		///<para>Filters the DirectoryItemViewModel collection based on a search query.
		/// The method uses a recursive function to search through the items and their subitems.</para>
		/// 
		/// <para>The recursive function, searchItems, takes a DirectoryItemViewModel as a parameter.
		/// It first checks if the item's name contains the query string, and sets a flag, isMatch, to 
		/// the result.</para>
		/// 
		/// <para>If the current item is a directory, the function iterates over its subitems and calls itself
		/// recursively for each subitem. If any of the subitems (or their subitems, and so on) contain 
		/// the query string, isMatch is set to true.</para>
		/// 
		/// <para>After checking the item and its subitems, the function updates the item's visibility and 
		/// expansion based on whether it matched the query. The item is visible and expanded if it or any 
		/// of its subitems matched the query, and collapsed otherwise. </para>
		/// 
		///<para> The function returns a boolean indicating whether the item or any of its subitems matched 
		///the query.</para>
		/// 
		/// <para>After defining the recursive function, the method applies it to each item in the
		/// DirectoryItemViewModel collection. This starts the recursive search for each item and its subitems.</para>
		/// </summary>
		/// <param name="queryObject">The search query as an object. It is cast to a string before being used.</param>
		public void FilterDirectoryItems(object queryObject)
		{
			// Little null check. 
			if (queryObject == null) return;
			string query = queryObject.ToString();
			
			if (DirectoryItemViewModel == null) return; // This is here so if the page is still loading and 
			// the user tries to search, it wont throw and error. 

			// Define a recursive function to search the items
			Func<DirectoryItemViewModel, bool> searchItems = null;
			searchItems = currentItem =>
			{
				bool isMatch = currentItem.Name.ToLower().Contains(query.ToLower());

				if (currentItem.IsDirectory)
				{
					// If the current item is a directory, recursively search its subitems
					foreach (var subItem in currentItem.SubItems)
					{
						if (searchItems(subItem))
						{
							isMatch = true;
						}
					}
				}

				// Update the visibility and expansion of the current item
				currentItem.Visibility = isMatch ? Visibility.Visible : Visibility.Collapsed;
				currentItem.IsExpanded = isMatch;

				return isMatch;
			};

			// Apply the recursive function to each item in the DirectoryItemViewModel collection
			foreach (var item in DirectoryItemViewModel)
			{
				searchItems(item);
			}
		}

		// Helper Methods
		/// <summary>
		/// Creates and displays a LoadingDialogView window. The window is positioned in the center 
		/// of the main application window. The method returns the created LoadingDialogView instance.
		/// </summary>
		/// <returns>The created <see cref="LoadingDialogView"/> instance.</returns>
		private LoadingDialogView ShowLoadingDialog()
		{
			var loadingDialog = new LoadingDialogView();

			var currentWindow = Application.Current.MainWindow;
			loadingDialog.Owner = Application.Current.MainWindow;

			loadingDialog.Left = currentWindow.Left + ((currentWindow.Width - loadingDialog.Width) / 2);
			loadingDialog.Top = currentWindow.Top + ((currentWindow.Height - loadingDialog.Height) / 2);

			loadingDialog.Show();
			loadingDialog.Owner = null;

			return loadingDialog;
		}

		/// <summary>
		/// Creates and displays an ErrorDialogView window with a specified error message. 
		/// The window is positioned in the center of the main application window.
		/// </summary>
		/// <param name="errorMessage">The error message to be displayed in the dialog.</param>
		private void ShowErrorDialog(string errorMessage)
		{
			var errorDialog = new ErrorDialogView();
			errorDialog.ErrorMessage.Text = errorMessage;

			var currentWindow = Application.Current.MainWindow;
			errorDialog.Owner = Application.Current.MainWindow;

			errorDialog.Left = currentWindow.Left + ((currentWindow.Width - errorDialog.Width) / 2);
			errorDialog.Top = currentWindow.Top + ((errorDialog.Height - errorDialog.Height) / 2);

			errorDialog.ShowDialog();
			errorDialog.Owner = null;
		}
	}
}
