// <copyright file="Sample17_GetImportJobs.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
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
			// destination workspace artifact Id.
			const int workspaceId = 1019056;

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
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IDocumentConfigurationController>())
			using (Relativity.Import.V1.Services.IImportJobController importJobController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())
			{
				// Create n import jobs.
				for (int i = 0; i < importCount; i++)
				{
					Guid importId = Guid.NewGuid();

					await importJobController.CreateAsync(
						importJobID: importId,
						workspaceID: workspaceId,
						applicationName: "Import-service-sample-app",
						correlationID: $"Sample-job-0017-GetImportJobs_{i}");

					await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
					await importJobController.BeginAsync(workspaceId, importId);
				}

				// Read import job collection (guid list) for particular workspace. Paginating is supported thanks to dedicated parameters.
				ValueResponse<ImportJobs> importJobs = await importJobController.GetJobsAsync(workspaceId, 0, pageSize);

				Console.WriteLine($"Import Jobs total count: {importJobs.Value.TotalCount}");
				Console.WriteLine("ImportJobIds:");
				foreach (var importJobId in importJobs.Value.Jobs)
				{
					Console.WriteLine(importJobId);
					await importJobController.EndAsync(workspaceId, importJobId);
				}
			}
		}
	}
}
