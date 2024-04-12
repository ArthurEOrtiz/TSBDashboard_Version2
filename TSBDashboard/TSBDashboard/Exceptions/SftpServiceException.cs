using System;

namespace TSBDashboard.Exceptions
{
	public class SftpServiceException : Exception
	{
		public SftpServiceException() { }

		public SftpServiceException(string message) : base(message) { }

		public SftpServiceException(string message, Exception innerException) : base(message, innerException) { }
	}
}
