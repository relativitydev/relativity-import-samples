// <copyright file="Sample17_GetImportJobs.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.Samples.DotNetFrameworkClient.ImportSampleHelpers;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models;
	using Relativity.Import.V1.Models.Settings;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading all import jobs Id.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample17_GetImportJobs()
		{
			Console.WriteLine($"Running {nameof(this.Sample17_GetImportJobs)}");

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			const int importCount = 10;
			const int pageSize = 7;
			const int controlNumberColumnIndex = 0;
			const int filePathColumnIndex = 11;
			const int fileNameColumnIndex = 13;

			// Configuration settings for document import. Builder is used to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number"))
				.WithoutFolders();

			using (Relativity.Import.V1.Services.IDocumentConfigurationController documentConfiguration =
				this.serviceFactory.CreateProxy<Relativity.Import.V1.Services.IDocumentConfigurationController>())
			using (Relativity.Import.V1.Services.IImportJobController importJobController =
				this.serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())
			{
				// Create n import jobs.
				Console.WriteLine($"Creating {importCount} jobs");
				for (int i = 0; i < importCount; i++)
				{
					Guid importId = Guid.NewGuid();

					var response = await importJobController.CreateAsync(
						importJobID: importId,
						workspaceID: workspaceId,
						applicationName: "Import-service-sample-app",
						correlationID: $"Sample-job-0017-GetImportJobs_{i}");

					ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CreateAsync");
				}

				// Read import job collection (guid list) for particular workspace. Paginating is supported thanks to dedicated parameters.
				ValueResponse<ImportJobs> valueResponse = await importJobController.GetJobsAsync(workspaceId, 0, pageSize);

				if (valueResponse.IsSuccess)
				{
					Console.WriteLine($"Import Jobs total count: {valueResponse.Value.TotalCount}");
					Console.WriteLine("ImportJobIds:");
					foreach (var importJobId in valueResponse.Value.Jobs)
					{
						Console.WriteLine(importJobId);
					}
				}
			}
		}
	}
}

/* Example of console result
	Response.IsSuccess: True
	Jobs total count: 10
	ImportJobIds:
	39753e22-a948-4c74-8ebd-3abd9fa47473
	8986cb61-8f1f-4ad7-96c0-8dc3f229fd1c
	33504c27-0bb2-46cd-9651-e539a4ae672f
	b83d8a04-320e-4256-93dc-4957e9908d14
	8b2ad1aa-c18e-4ccd-a1b3-9cacdf2d1ce6
	1bac695d-bab2-41fb-a6f7-0d995c5e6871
	a98b009e-01b1-4b3c-96b4-491d33fb5827
*/