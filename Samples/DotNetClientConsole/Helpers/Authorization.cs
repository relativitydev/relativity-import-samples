// <copyright file="Authorization.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetClient.Helpers
{
	using System;
	using System.Net.Http.Headers;
	using System.Text;

	/// <summary>
	/// Helper class for authorization.
	/// </summary>
	internal class Authorization
	{
		/// <summary>
		/// Gets basic authentication header value.
		/// </summary>
		/// <returns>Instance of AuthenticationHeaderValue.</returns>
		public static AuthenticationHeaderValue GetBasicAuthenticationHeaderValue()
		{
			// Basic authentication method was applied for samples purpose.
			// See Relativity REST API authentication documentation describing other authentication methods.
			// https://platform.relativity.com/RelativityOne/Content/REST_API/REST_API_authentication.htm
			var authenticationString = $"{RelativityUserSettings.UserName}:{RelativityUserSettings.Password}";
			var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

			return new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
		}
	}
}
