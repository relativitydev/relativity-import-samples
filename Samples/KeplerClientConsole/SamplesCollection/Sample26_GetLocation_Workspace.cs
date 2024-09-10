// <copyright file="Sample26_GetLocation_Workspace.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.Samples.DotNetFrameworkClient.ImportSampleHelpers;
	using Relativity.Import.V1;

	/// <summary>
	/// Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of getting default import location for workspace.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample26_GetLocation_Workspace()
		{
			// Targetted workspace ID
			int workspaceId = 1000000;

			// Create proxy for IImportJobController
			using (var importService = this.serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())
			{
				// Send the request
				ValueResponse<string> response = await importService.GetDefaultLocationForWorkspaceAsync(workspaceId);

				ResponseHelper.EnsureSuccessResponse(response, "Get location for workspace");

				if (response?.IsSuccess ?? false)
				{
					Console.WriteLine("Location for workspace with ID {0} is: {1}", workspaceId, response.Value);
				}
			}
		}
	}
}
