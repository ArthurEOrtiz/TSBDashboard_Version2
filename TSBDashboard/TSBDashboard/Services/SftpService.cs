using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using TSBDashboard.Exceptions;
using TSBDashboard.Options;
using TSBDashboard.ViewModels;
using WinSCP;

namespace TSBDashboard.Services
{
	public class SftpService
	{
		private readonly SftpSettings _sftpSettings;
		private Session _session = null;
		private string UserName;
		private SecureString Password;

		public SftpService(IOptions<SftpSettings> sftpSettings)
		{
			_sftpSettings = sftpSettings.Value;
			App.ApplicationExiting += CloseSession;
		}

		~SftpService()
		{
			Dispose(false);
		}

		public void SetUserNamePassWord(string userName, SecureString password)
		{
			UserName = userName;
			Password = password;
		}

		/// <summary>
		/// Establishes a connection to the SFTP server using the provided username and password.
		/// The method uses the host and SSH host key fingerprint provided during the initialization of the SftpService.
		/// If the connection is successful, a new Session is opened.
		/// If the authentication fails, an AuthenticationFailedException is thrown.
		/// If any other error occurs during the login process, a SftpServiceException is thrown.
		/// </summary>
		/// <param name="userName">The username to use for authentication with the SFTP server.</param>
		/// <param name="password">The password to use for authentication with the SFTP server. This is a SecureString for added security.</param>
		/// <exception cref="AuthenticationFailedException">Thrown when the provided username or password is incorrect.</exception>
		/// <exception cref="SftpServiceException">Thrown when an error occurs while trying to establish a connection to the SFTP server.</exception>
		public void LogIn()
		{
			foreach (var host in _sftpSettings.Hosts)
			{
				try
				{
					var sessionOptions = new SessionOptions()
					{
						Protocol = Protocol.Sftp,
						HostName = host,
						PortNumber = 22,
						UserName = $"ptaxtransfer.idaho.gov|{UserName}",
						SecurePassword = Password,
						SshHostKeyFingerprint = _sftpSettings.SshHostKeyFingerPrint
					};

					_session = new Session();
					_session.Open(sessionOptions);
					break;
				}
				catch (SessionRemoteException ex)
				{
					if (ex.Message.Contains("Authentication failed"))
					{
						throw new AuthenticationFailedException("Invalid username or password.", ex);
					}
					else
					{
						continue;
					}
				}
				catch (Exception ex)
				{
					throw new SftpServiceException("Error connecting to SFTP server.", ex);
				}
			}
		}

		/// <summary>
		/// Asynchronously retrieves all directories and files from a specified path.
		/// This method uses recursion to traverse all subdirectories and files.
		/// Each directory or file is represented as a DirectoryItemViewModel object.
		/// </summary>
		/// <param name="path">The path from which to retrieve directories and files.</param>
		/// <returns>A list of DirectoryItemViewModel objects representing all directories and files in the specified path.</returns>
		/// <exception cref="SftpServiceException">Thrown when an error occurs during the retrieval of directories and files.</exception>
		public async Task<List<DirectoryItemViewModel>> GetAllDirectoriesAndFiles(string path)
		{
			var items = new List<DirectoryItemViewModel>();

			try
			{
				RemoteDirectoryInfo rootDirectory = await LoadDirectory(path);
				foreach (RemoteFileInfo file in rootDirectory.Files)
				{
					if (file.Name != "." && file.Name != "..")
					{
						var Item = new DirectoryItemViewModel()
						{
							Name = file.Name,
							Path = file.FullName,
							IsDirectory = file.IsDirectory,
						};
						items.Add(Item);

						if (file.IsDirectory)
						{
							Item.SubItems = await GetAllDirectoriesAndFiles(file.FullName);
						}

					}
				}
				return items;
			}
			catch (Exception ex)
			{
				throw new SftpServiceException(path, ex);
			}
		}

		public async Task DownloadFileAsync(string fileName, string path, int retryCount = 1)
		{
			string destinationPath = GetDestinationPath(fileName);

			if (!IsSessionOpen() || retryCount == 0)
			{
				CloseSession();
				LogIn();
			}

			try
			{
				await Task.Run(() => _session.GetFiles(path, destinationPath, false));
			}
			catch (Exception ex)
			{
				if (retryCount > 0)
				{
					await DownloadFileAsync(fileName, path, retryCount - 1);
				}
				else
				{
					throw new SftpServiceException("Failed to download file, please contact support.", ex);
				}
			}
		}

		/// <summary>
		/// Opens a file using the appropriate service based on the file's extension.
		/// </summary>
		/// <param name="destinationPath">The path of the file to open.</param>
		/// <remarks>
		/// If the file has a ".rpt" extension, it is opened with the CrystalReportsViewerService.
		/// If the file has a ".sql" extension, it is opened with the ScriptRunnerService.
		/// If the file has any other extension, it is opened with the default program associated with that extension.
		/// </remarks>
		/// <exception cref="SftpServiceException">Thrown when an error occurs while trying to open the file.</exception>
		public void OpenFile(string fileName)
		{
			string destinationPath = GetDestinationPath(fileName);
			var extension = Path.GetExtension(destinationPath);

			try
			{
				if (extension != ".rpt" && extension != ".sql")
				{
					Process.Start(new ProcessStartInfo(destinationPath) { UseShellExecute = true });
				}

				if (extension == ".rpt")
				{
					new CrystalReportsViewerService(destinationPath).ShowReport();
				}

				if (extension == ".sql")
				{
					new ScriptRunnerService(destinationPath).ExecuteProgram();
				}
			}
			catch (Exception ex)
			{
				throw new SftpServiceException($"Error opening {destinationPath}", ex);
			}
		}

		/// <summary>
		/// Asynchronously loads a directory from a specified path.
		/// This method uses the SSH.NET library's ListDirectory method to retrieve the directory.
		/// </summary>
		/// <param name="path">The path of the directory to load.</param>
		/// <returns>A RemoteDirectoryInfo object representing the loaded directory.</returns>
		/// <exception cref="SftpServiceException">Thrown when an error occurs during the loading of the directory.
		/// </exception>
		private async Task<RemoteDirectoryInfo> LoadDirectory(string path)
		{
			try
			{
				// Use the SSH.NET library's ListDirectory method to retrieve the directory
				RemoteDirectoryInfo remoteDirectoryInfo = await Task.Run(() => _session.ListDirectory(path));

				// Return the RemoteDirectoryInfo object
				return remoteDirectoryInfo;
			}
			catch (Exception ex)
			{
				// Rethrow the exception to be handled by the caller
				throw new SftpServiceException($"Error loading directory {path}", ex);
			}
		}

		/// <summary>
		/// Closes Session if it is open. 
		/// </summary>
		public void CloseSession()
		{
			if (_session != null)
			{
				_session.Close();
				_session = null;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}	

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				App.ApplicationExiting -= CloseSession;
				CloseSession();
			}
		}

		private bool IsSessionOpen()
		{
			return _session != null && _session.Opened;
		}


		private string GetDestinationPath(string fileName)
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string myAppDataPath = Path.Combine(appDataPath, "TSBDashboard");
			return Path.Combine(myAppDataPath, fileName);
		}
	}
}
