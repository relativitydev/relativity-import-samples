using Relativity.Services.ServiceProxy;
using System;

namespace Relativity.Import.Samples.DotNetFrameworkClient.ImportSampleHelpers
{
	public static class KeplerProxyHelper
	{
		/// Helper method create ServiceFactory class.
		/// Use the ServiceFactory class to create proxies for Kepler services. See documentation for details:
		/// https://platform.relativity.com/RelativityOne/Content/Kepler_framework/Proxies_and_authentication.htm#Service .

		public static IServiceFactory GetServiceFactory()
		{
			var relativityRestUri = new Uri($"{RelativityUserSettings.HostAddress}/relativity.rest/api");
			Credentials credentials = new UsernamePasswordCredentials(RelativityUserSettings.UserName, RelativityUserSettings.Password);

			var settings = new ServiceFactorySettings(relativityRestUri, credentials);

			return new ServiceFactory(settings);
		}
	}
}
