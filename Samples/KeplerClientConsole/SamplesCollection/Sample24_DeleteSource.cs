// <copyright file="Sample24_DeleteSource.cs" company="Relativity ODA LLC">
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
		/// Example of deleting a folder associated with a specific workspace, job, and source in Relativity.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample24_DeleteSource()
		{
			// Targetted workspace ID
			int workspaceId = 100000;

			// Targetted job GUID
			Guid jobID = Guid.Parse("00000000-0000-0000-0000-000000000000");

			// Targetted job source GUID
			Guid sourceID = Guid.Parse("00000000-0000-0000-0000-000000000000");

			// Create proxy for IImportSourceController
			using (var importService = this.serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportSourceController>())
			{
				// Send the request
				Response response = await importService.DeleteParentFolderAsync(workspaceId, jobID, sourceID);

				ResponseHelper.EnsureSuccessResponse(response, "Get location for workspace");

				if (response?.IsSuccess ?? false)
				{
					Console.WriteLine("Folder deleted for {workspaceId}", workspaceId);
					Console.WriteLine("Job ID: {jobID}", jobID);
					Console.WriteLine("Source ID: {sourceID}", sourceID);
				}
			}
		}
	}
}
