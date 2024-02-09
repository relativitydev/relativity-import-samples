// <copyright file="Sample18_GetDataSource.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.Samples.DotNetFrameworkClient.ImportSampleHelpers;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.DataSource;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading all data sources Ids for particular job.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample18_GetDataSource()
		{
			Console.WriteLine($"Running {nameof(this.Sample18_GetDataSource)}");

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			const int dataSourceCount = 10;

			// set of columns indexes in load file used in import settings.
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
			using (Relativity.Import.V1.Services.IImportSourceController importSourceController =
				this.serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportSourceController>())
			{
				// Create import job.
				Guid importId = Guid.NewGuid();
				var response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: $"Sample-job-0018");
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CreateAsync");

				// Add import document settings to existing import job.
				await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				ResponseHelper.EnsureSuccessResponse(response, "IDocumentConfigurationController.CreateAsync");

				// Add n data sources to the existing job.
				for (var i = 0; i < dataSourceCount; i++)
				{
					var dataSourceId = Guid.NewGuid();

					DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
						.ForLoadFile($"host/Sample18/loadFile{i}")
						.WithDefaultDelimiters()
						.WithFirstLineContainingHeaders()
						.WithEndOfLineForWindows()
						.WithStartFromBeginning()
						.WithDefaultEncoding()
						.WithDefaultCultureInfo();

					await importSourceController.AddSourceAsync(workspaceId, importId, dataSourceId, dataSourceSettings);
					ResponseHelper.EnsureSuccessResponse(response, "IImportSourceController.AddSourceAsync");
				}

				// Read collection of data sources for particular import job.
				ValueResponse<DataSources> valueResponse = await importJobController.GetSourcesAsync(workspaceId, importId);
				if (valueResponse.IsSuccess)
				{
					Console.WriteLine($"Data Sources total count: {valueResponse.Value.TotalCount}");
					Console.WriteLine("Data source Ids:");
					foreach (var sources in valueResponse.Value.Sources)
					{
						Console.WriteLine(sources);
					}
				}
			}
		}
	}
}

/* Example of console result:
	Response.IsSuccess: True
	Data Sources total count: 10
	Data source Ids:
	21b918c4-4bbf-4e1b-b90a-953e23721aa5
	30386162-ad65-434f-9f01-b898b092e0f4
	36b3f57e-c0d2-462d-bd86-13e1fbb7ff9c
	44e7eeb3-ef83-4dc0-83d2-8cfcec1ee366
	45eeb401-0e22-420a-8a8d-206b4a3c1dc8
	988b7d83-e9b2-436c-b742-d5bd6fc57960
	b32fcf4d-08f0-4220-a6a4-e16864ec84f1
	bfd59462-e60d-4459-a964-8c85f82f1d9e
	d74abffb-34a5-4efe-9b4b-01774e5ccfcf
	f05de81a-18c0-4bb8-b19b-52fd080187ac
*/