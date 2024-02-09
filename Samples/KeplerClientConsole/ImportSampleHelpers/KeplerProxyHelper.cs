// <copyright file="KeplerProxyHelper.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetFrameworkClient.ImportSampleHelpers
{
	using System;
	using Relativity.Services.ServiceProxy;

	/// <summary>
	/// Helper class for kepler proxy.
	/// </summary>
	public static class KeplerProxyHelper
	{
		// Helper method create ServiceFactory class.
		// Use the ServiceFactory class to create proxies for Kepler services. See documentation for details:
		// https://platform.relativity.com/RelativityOne/Content/Kepler_framework/Proxies_and_authentication.htm#Service .

		/// <summary>
		/// Get service factory.
		/// </summary>
		/// <returns>ServiceFactory that can return a proxy to a given service interface.</returns>
		public static IServiceFactory GetServiceFactory()
		{
			var relativityRestUri = new Uri($"{RelativityUserSettings.HostAddress}/relativity.rest/api");
			Credentials credentials = new UsernamePasswordCredentials(RelativityUserSettings.UserName, RelativityUserSettings.Password);

			var settings = new ServiceFactorySettings(relativityRestUri, credentials);

			return new ServiceFactory(settings);
		}
	}
}
