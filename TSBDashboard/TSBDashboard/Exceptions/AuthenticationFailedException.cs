using System;

namespace TSBDashboard.Exceptions
{
	public class AuthenticationFailedException : SftpServiceException
	{
		public AuthenticationFailedException() { }

		public AuthenticationFailedException(string message) : base(message) { }

		public AuthenticationFailedException(string message, Exception innerException) : base(message, innerException) { }
	}
}
