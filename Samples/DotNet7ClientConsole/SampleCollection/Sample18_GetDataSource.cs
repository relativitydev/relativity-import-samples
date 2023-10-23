// <copyright file="Sample18_GetDataSource.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.Net7Client.SampleCollection
{
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
	using System.Net.Http.Json;
	using Relativity.Import.V1.Models;
	using Relativity.Import.Samples.Net7Client.Helpers;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Builders.DataSource;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading data source Ids.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample18_GetDataSource()
		{
			Console.WriteLine($"Running {nameof(Sample18_GetDataSource)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();

			// Destination workspace artifact Id.
			const int workspaceId = 1000000;

			// set of columns indexes in load file used in import settings.
			const int controlNumberColumnIndex = 0;
			const int dataSourceCount = 10;
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


			// Create payload for request.
			var importSettingPayload = new { importSettings };

			HttpClient httpClient = HttpClientHelper.CreateHttpClient();

			// Create payload for request.
			var createJobPayload = new
			{
				applicationName = "Import-service-sample-app",
				correlationID = $"Sample-job-0018"
			};

			// Create import job.
			// endpoint: POST /import-jobs/{importId}
			var createImportJobUri = RelativityImportEndpoints.GetImportJobCreateUri(workspaceId, importId);

			var response = await httpClient.PostAsJsonAsync(createImportJobUri, createJobPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Add import document settings to existing import job (configure import job).
			// endpoint: POST /import-jobs/{importId}/documents-configurations
			var documentConfigurationUri = RelativityImportEndpoints.GetDocumentConfigurationUri(workspaceId, importId);
			response = await httpClient.PostAsJsonAsync(documentConfigurationUri, importSettingPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);


			// Add n data sources to the existing job.
			for (int i = 0; i < dataSourceCount; i++)
			{
				Guid dataSourceId = Guid.NewGuid();

				var dataSourceSettings = DataSourceSettingsBuilder.Create()
					.ForLoadFile($"host/Sample18/loadFile{i}")
					.WithDefaultDelimiters()
					.WithFirstLineContainingHeaders()
					.WithEndOfLineForWindows()
					.WithStartFromBeginning()
					.WithDefaultEncoding()
					.WithDefaultCultureInfo();

				// Create payload for request.
				var dataSourceSettingsPayload = new { dataSourceSettings };

				// Add data source settings to existing import job.
				// endpoint: POST /import-jobs/{importId}/sources/{sourceId}
				var importSourceUri = RelativityImportEndpoints.GetImportSourceUri(workspaceId, importId, dataSourceId);
				response = await httpClient.PostAsJsonAsync(importSourceUri, dataSourceSettingsPayload);
				await ImportJobSampleHelper.EnsureSuccessResponse(response);
			}

			// Read collection of data sources for particular import job.
			var importSourcesUri = RelativityImportEndpoints.GetImportSourcesForJobUri(workspaceId, importId);
			ValueResponse<DataSources>? valueResponse =
				await httpClient.GetFromJsonAsync<ValueResponse<DataSources>>(importSourcesUri);

			if (valueResponse is {IsSuccess: true})
			{
				Console.WriteLine($"Data Sources total count: {valueResponse.Value.TotalCount}");
				Console.WriteLine($"Data source Ids:");
				foreach (var source in valueResponse.Value.Sources)
				{
					Console.WriteLine(source);
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