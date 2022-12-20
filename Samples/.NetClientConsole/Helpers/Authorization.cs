// <copyright file="Authorization.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System;
using System.Text;

namespace Relativity.Import.Samples.Net7Client.Helpers
{
	using System.Net.Http.Headers;

	internal class Authorization
	{
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
