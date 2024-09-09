// <copyright file="Sample25_GetLocation.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using Relativity.Import.Samples.DotNetFrameworkClient.ImportSampleHelpers;

using System;
using System.Threading.Tasks;

namespace Relativity.Import.Samples.DotNetFrameworkClient.SamplesCollection
{
	/// <summary>
	/// Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of getting default location for import job.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample25_GetLocation_Job()
		{
			// Targetted workspace ID
			int workspaceId = 1000000;

			// Targetted job GUID
			Guid jobGuid = Guid.Parse("00000000-0000-0000-0000-000000000000");

			// Create proxy for IImportJobController
			using (var importService = this.serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())
			{
				// Send te request
				var response = await importService.GetDefaultLocationForJobAsync(workspaceId, jobGuid);

				ResponseHelper.EnsureSuccessResponse(response, "Get location for workspace");

				if (response?.IsSuccess ?? false)
				{
					Console.WriteLine("Location for workspace with ID {0} is: {1}", workspaceId, response.Value);
				}
			}
		}
	}
}
