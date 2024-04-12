namespace TSBDashboard.Options
{
	/// <summary>
	/// Represents the settings for SFTP connection.
	/// </summary>
	public class SftpSettings
	{
		/// <summary>
		/// Gets or sets the host name or IP address of the SFTP server.
		/// </summary>
		public string Host { get; set; }

		/// <summary>
		/// Gets or sets the SSH host key fingerprint for the SFTP server.
		/// </summary>
		public string SshHostKeyFingerPrint { get; set; }
	}
}
