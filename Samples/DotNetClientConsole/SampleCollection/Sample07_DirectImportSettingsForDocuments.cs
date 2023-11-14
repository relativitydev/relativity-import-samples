// <copyright file="Sample07_DirectImportSettingsForDocuments.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetClient.SampleCollection
{
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
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
		/// Example of setting ImportDocumentSetting manually (without ImportDocumentSettingsBuilder).
		/// Settings dedicated for image import.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample07_DirectImportSettingsForDocuments()
		{
			Console.WriteLine($"Running {nameof(Sample07_DirectImportSettingsForDocuments)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace and folder artifact Ids.
			const int workspaceId = 1000000;
			const int rootFolderId = 2000000;

			// overlay keyField
			const string keyField = "Control Number";

			// set of columns indexes in load file used in import settings.
			const int extractedTextFilePathColumnIndex = 12;
			const int emailToColumnIndex = 11;
			const int fileNameColumnIndex = 13;
			const int fileSizeColumnIndex = 14;
			const int filePathColumnIndex = 22;

			// Create payload for request.
			var createJobPayload = new
			{
				applicationName = "Import-service-sample-app",
				correlationID = "Sample-job-0007"
			};

			// Configuration settings for document import. Example of set without using ImportDocumentSettingsBuilder.
			ImportDocumentSettings importSettings = new ImportDocumentSettings()
			{
				Overlay = new OverlaySettings
				{
					Mode = OverlayMode.AppendOverlay,
					KeyField = keyField,
					MultiFieldOverlayBehaviour = MultiFieldOverlayBehaviour.UseRelativityDefaults,
				},
				Native = new NativeSettings
				{
					FileNameColumnIndex = fileNameColumnIndex,
					FilePathColumnIndex = filePathColumnIndex,
				},
				Fields = new FieldsSettings
				{
					FieldMappings = new[]
					{
						new FieldMapping
						{
							Field = "Control Number",
							ContainsID = false,
							ColumnIndex = 0,
							ContainsFilePath = false,
						},
						new FieldMapping
						{
							Field = "Custodian - Single Choice",
							ContainsID = false,
							ColumnIndex = 1,
							ContainsFilePath = false,
						},
						new FieldMapping
						{
							Field = "Email To",
							ContainsID = false,
							ColumnIndex = emailToColumnIndex,
							ContainsFilePath = false,
						},
						new FieldMapping
						{
							Field = "Extracted Text",
							ContainsID = false,
							ColumnIndex = extractedTextFilePathColumnIndex,
							ContainsFilePath = true,
							Encoding = "UTF-8",
							FileSizeColumnIndex = fileSizeColumnIndex
						},
					},
				},
				Folder = new FolderSettings
				{
					FolderPathColumnIndex = null,
					RootFolderID = rootFolderId,
				}
			};

			// Create payload for request.
			var importSettingPayload = new { importSettings };

			// Example of data source configuration created without using DataSourceSettingsBuilder.
			DataSourceSettings dataSourceSettings = new DataSourceSettings
			{
				Type = DataSourceType.LoadFile,
				Path = "\\files\\T001\\Files\\SampleDataSources\\load_file_04.dat",
				NewLineDelimiter = '#',
				ColumnDelimiter = '|',
				QuoteDelimiter = '^',
				MultiValueDelimiter = '$',
				NestedValueDelimiter = '&',
				Encoding = null,
				CultureInfo = "en-us",
				EndOfLine = DataSourceEndOfLine.Windows,
				FirstLineContainsColumnNames = true,
				StartLine = 0,
			};

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
				Converters = { new JsonStringEnumConverter() }
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