using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
		private readonly string _host;
		private readonly string _sshHostKeyFingerPrint;
		private Session _session = null;

		public SftpService(IOptions<SftpSettings> sftpSettings)
		{
			_host = sftpSettings.Value.Host;
			_sshHostKeyFingerPrint = sftpSettings.Value.SshHostKeyFingerPrint;
			App.ApplicationExiting += CloseSession;
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
		public void LogIn(string userName, SecureString password)
		{
			try
			{
				var sessionOptions = new SessionOptions()
				{
					Protocol = Protocol.Sftp,
					HostName = _host,
					UserName = $"{_host}|{userName}",
					Password = ConvertToUnsecureString(password),
					SshHostKeyFingerprint = _sshHostKeyFingerPrint
				};

				_session = new Session();
				_session.Open(sessionOptions);
			}
			catch (SessionRemoteException ex)
			{
				if (ex.Message.Contains("Authentication failed"))
				{
					throw new AuthenticationFailedException("Invalid username or password.", ex);
				}
				else
				{
					throw new SftpServiceException("An error occurred while trying to log in.", ex);
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

		/// <summary>
		/// <para>
		/// Asynchronously downloads a file from an SFTP server.
		/// </para>
		/// <para>
		/// This method is responsible for connecting to the server, navigating to the correct directory,
		/// and downloading the specified file. It may also handle errors and exceptions that occur during 
		/// the process. Additionally It will open the file upon download via the <seealso cref="OpenFile(string)"/>
		/// </para>
		/// <para>
		/// The method is asynchronous, meaning it returns a Task that represents the ongoing download 
		/// operation. This allows the method to be awaited, so it can be run in the background without
		/// blocking the main application thread.
		/// </para>
		/// </summary>
		/// <param name="remoteFilePath">The path to the file on the SFTP server.</param>
		/// <param name="localFilePath">The path where the file should be saved on the local machine.</param>
		/// <returns>A Task representing the ongoing download operation.</returns>
		/// <exception cref="SftpServiceException">Thrown when the there has been an error downloading the file.
		/// This will be handle by the UI and should update the user about it.</exception>
		public async Task DownloadFileAsync(string fileName, string path)
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string myAppDataPath = Path.Combine(appDataPath, "TSBDashboard");
			string destinationPath = Path.Combine(myAppDataPath, fileName);
			try
			{
				await Task.Run(() => _session.GetFiles(path, destinationPath, false));

				// Open the file after it has been downloaded
				_ = Task.Run(() => OpenFile(destinationPath));
			}
			catch (Exception ex)
			{
				throw new SftpServiceException("DownloadFile Error", ex);	
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
		private void OpenFile(string destinationPath)
		{
			var extension = Path.GetExtension(destinationPath);
			try
			{
				if (extension != ".rpt" && extension != ".sql")
				{
					Process.Start(new ProcessStartInfo(destinationPath) { UseShellExecute = true });
				}

				if (extension == ".rpt")
				{
					new CrystalReportsViewerService(destinationPath).ExecuteProgram();
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
			if (_session != null && _session.Opened)
			{
				_session.Close();
			}
		}

		/// <summary>
		/// Converts a <see cref="SecureString"/> to an unsecure string.
		/// </summary>
		/// <param name="securePassword">The <see cref="SecureString"/> to convert.</param>
		/// <returns>The unsecure string representation of the <see cref="SecureString"/>.</returns>
		private string ConvertToUnsecureString(SecureString securePassword)
		{
			if (securePassword == null)
			{
				return string.Empty;
			}

			IntPtr unmanagedString = IntPtr.Zero;
			try
			{
				unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
				return Marshal.PtrToStringUni(unmanagedString);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
			}
		}
	}
}
