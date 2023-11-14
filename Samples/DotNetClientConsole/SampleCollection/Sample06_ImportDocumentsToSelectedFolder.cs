// <copyright file="Sample06_ImportDocumentsToSelectedFolder.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetClient.SampleCollection
{
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.DataSource;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;
	using System.Net.Http.Json;
	using Relativity.Import.V1.Models;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using Relativity.Import.Samples.DotNetClient.Helpers;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of settings used to import documents to selected folder under the workspace.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample06_ImportDocumentsToSelectedFolder()
		{
			Console.WriteLine($"Running {nameof(Sample06_ImportDocumentsToSelectedFolder)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			// destination folder artifact id.
			const int rootFolderId = 2000000;

			// set of columns indexes in load file used in import settings.
			const int folderPathColumnIndex = 15;
			const int controlNumberColumnIndex = 0;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Path to the load file used in data source settings.
			const string loadFile03Path = "\\files\\<TenantNumber>\\Files\\SampleDataSources\\load_file_03.dat";

			// Create payload for request.
			var createJobPayload = new
			{
				applicationName = "Import-service-sample-app",
				correlationID = "Sample-job-0006"
			};

			// Configuration settings for document import. Builder is used to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number"))
				.WithFolders(f => f
					.WithRootFolderID(rootFolderId, r => r
						.WithFolderPathDefinedInColumn(folderPathColumnIndex)));


			// Create payload for request.
			var importSettingPayload = new { importSettings };

			// Configuration settings for data source. Builder is used to create settings.
			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForLoadFile(loadFile03Path)
				.WithDefaultDelimiters()
				.WithoutFirstLineContainingHeaders()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();

			// Create payload for request.
			var dataSourceSettingsPayload = new { dataSourceSettings };

			HttpClient httpClient = HttpClientHelper.CreateHttpClient();

			// Create import job.
			// endpoint: POST /import-jobs/{importId}
			var createImportJobUri = RelativityImportEndpoints.GetImportJobCreateUri(workspaceId, importId);

			var response = await httpClient.PostAsJsonAsync(createImportJobUri,createJobPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Add import document settings to existing import job (configure import job).
			// endpoint: POST /import-jobs/{importId}/documents-configurations
			var documentConfigurationUri = RelativityImportEndpoints.GetDocumentConfigurationUri(workspaceId, importId);
			response = await httpClient.PostAsJsonAsync(documentConfigurationUri, importSettingPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Add data source settings to existing import job.
			// endpoint: POST /import-jobs/{importId}/sources/{sourceId}
			var importSourcesUri = RelativityImportEndpoints.GetImportSourceUri(workspaceId, importId, sourceId);
			response = await httpClient.PostAsJsonAsync(importSourcesUri, dataSourceSettingsPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Start import job.
			// endpoint: POST /import-jobs/{importId}/begin
			var beginImportJobUri = RelativityImportEndpoints.GetImportJobBeginUri(workspaceId, importId);
			response = await httpClient.PostAsync(beginImportJobUri, null);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// End import job.
			// endpoint: POST /import-jobs/{importId}/end
			var endImportJobUri = RelativityImportEndpoints.GetImportJobEndUri(workspaceId, importId);
			response = await httpClient.PostAsync(endImportJobUri, null);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// It may take some time for import job to be completed. Request data source details to monitor the current state.
			// NOTE: You can also request job details to verify if job is finished - see appropriate sample.
			// endpoint: GET import-jobs/{importId}/sources/{sourceId}/details"
			var importSourceDetailsUri = RelativityImportEndpoints.GetImportSourceDetailsUri(workspaceId, importId, sourceId);

			JsonSerializerOptions options = new()
			{
				Converters = { new JsonStringEnumConverter() },
				IncludeFields = true
			};

			var dataSourceState = await ImportJobSampleHelper.WaitImportDataSourceToBeCompleted(
				funcAsync: () => httpClient.GetFromJsonAsync<ValueResponse<DataSourceDetails>> (importSourceDetailsUri, options),
				timeout: 10000);

			// Get current import progress for specific data source.
			// endpoint: GET import-jobs/{importId}/sources/{sourceId}/progress"
			var importSourceProgressUri = RelativityImportEndpoints.GetImportSourceProgressUri(workspaceId, importId, sourceId);

			var valueResponse = await httpClient.GetFromJsonAsync<ValueResponse<ImportProgress>>(importSourceProgressUri);
			
			if (valueResponse?.IsSuccess ?? false)
			{
				Console.WriteLine("\n");
				Console.WriteLine($"Data source state: {dataSourceState}");
				Console.WriteLine($"Import data source progress: Total records: {valueResponse.Value.TotalRecords}, Imported records: {valueResponse.Value.ImportedRecords}, Records with errors: {valueResponse.Value.ErroredRecords}");
			}
		}
	}
}

/* Expected console result:
	Data source state: Completed
	Import data source progress: Total records: 2, Imported records: 2, Records with errors: 0
 */
