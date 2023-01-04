// <copyright file="Sample03_ImportFromTwoDataSources.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.Net7Client.SampleCollection
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
	using System.Text.Json.Serialization;
	using System.Text.Json;
	using Relativity.Import.Samples.Net7Client.Helpers;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of using two data sources to import documents into workspace.
		/// In this example second data sources is added after import job is started.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample04_AddDataSourceToRunningJob()
		{
			Console.WriteLine($"Running {nameof(Sample04_AddDataSourceToRunningJob)}");
			// GUID identifiers for import job and data sources.
			Guid importId = Guid.NewGuid();
			Guid source01Id = Guid.NewGuid();
			Guid source02Id = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			// set of columns indexes in load file used in import settings.
			const int controlNumberColumnIndex = 0;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Path to the load files used in data source settings.
			const string loadFile01Path = "C:\\DefaultFileRepository\\samples\\load_file_01.dat";
			const string loadFile02Path = "C:\\DefaultFileRepository\\samples\\load_file_02.dat";

			// Create payload for request.
			var createJobPayload = new
			{
				applicationName = "Import-service-sample-app",
				correlationID = "Sample-job-0004"
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
				.WithoutFolders();

			// Create payload for request.
			var importSettingPayload = new { importSettings };

			// Configuration settings for data source. Builder is used to create settings.
			DataSourceSettings dataSourceSettings01 = DataSourceSettingsBuilder.Create()
				.ForLoadFile(loadFile01Path)
				.WithDelimiters(d => d
					.WithColumnDelimiters('|')
					.WithQuoteDelimiter('^')
					.WithNewLineDelimiter('#')
					.WithNestedValueDelimiter('&')
					.WithMultiValueDelimiter('$'))
				.WithFirstLineContainingHeaders()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();

			// Configuration settings for second data source. Builder is used to create settings.
			DataSourceSettings dataSourceSettings02 = DataSourceSettingsBuilder.Create()
				.ForLoadFile(loadFile02Path)
				.WithDelimiters(d => d
					.WithColumnDelimiters('|')
					.WithQuoteDelimiter('^')
					.WithNewLineDelimiter('#')
					.WithNestedValueDelimiter('&')
					.WithMultiValueDelimiter('$'))
				.WithoutFirstLineContainingHeaders()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();

			// Create payloads for requests.
			var dataSourceSettingsPayload01 = new { dataSourceSettings = dataSourceSettings01 };
			var dataSourceSettingsPayload02 = new { dataSourceSettings = dataSourceSettings02 };

			HttpClient httpClient = HttpClientHelper.CreateHttpClient();

			// Create import job.
			// endpoint: POST /import-jobs/{importId}
			var createImportJobUri = RelativityImportEndpoints.GetCreateImportUri(workspaceId, importId);
			var response = await httpClient.PostAsJsonAsync(createImportJobUri, createJobPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Add import document settings to existing import job (configure import job).
			// endpoint: POST /import-jobs/{importId}/documents-configurations
			var documentConfigurationUri = RelativityImportEndpoints.GetDocumentConfigurationUri(workspaceId, importId);
			response = await httpClient.PostAsJsonAsync(documentConfigurationUri, importSettingPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Add first data source settings to existing import job.
			// endpoint: POST /import-jobs/{importId}/sources/{sourceId}
			var importSourcesUri = RelativityImportEndpoints.GetImportSourceUri(workspaceId, importId, source01Id);
			response = await httpClient.PostAsJsonAsync(importSourcesUri, dataSourceSettingsPayload01);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Start import job.
			// endpoint: POST /import-jobs/{importId}/begin
			var beginImportJobUri = RelativityImportEndpoints.GetBeginJobUri(workspaceId, importId);
			response = await httpClient.PostAsync(beginImportJobUri, null);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Add second data source settings to existing import job.
			// Both data sources are added before job starts.
			importSourcesUri = RelativityImportEndpoints.GetImportSourceUri(workspaceId, importId, source02Id);
			response = await httpClient.PostAsJsonAsync(importSourcesUri, dataSourceSettingsPayload02);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// End import job.
			// endpoint: POST /import-jobs/{importId}/end
			var endImportJobUri = RelativityImportEndpoints.GetEndJobUri(workspaceId, importId);
			response = await httpClient.PostAsync(endImportJobUri, null);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// It may take some time for import job to be completed. Request import job details to monitor the current state.
			// You can also get data sources details to verify if all sources are imported.
			var importDetailsUri = RelativityImportEndpoints.GetImportDetailsUri(workspaceId, importId);

			JsonSerializerOptions options = new()
			{
				Converters = { new JsonStringEnumConverter() }
			};

			var importState = await ImportJobSampleHelper.WaitImportJobToBeFinished(
				funcAsync: () => httpClient.GetFromJsonAsync<ValueResponse<ImportDetails>>(importDetailsUri, options),
				timeout: 10000);

			Console.WriteLine($"\nImport job state: {importState}");

			// Get current import progress for specific data source.
			// endpoint: GET import-jobs/{importId}/sources/{sourceId}/progress"
			foreach (var sourceId in new[] { source01Id, source02Id })
			{
				var importSourceProgressUri = RelativityImportEndpoints.GetImportSourceProgressUri(workspaceId, importId, sourceId);

				var valueResponse = await httpClient.GetFromJsonAsync<ValueResponse<ImportProgress>>(importSourceProgressUri);

				if (valueResponse?.IsSuccess ?? false)
				{
					Console.WriteLine($"Import data source progress (sourceID: {sourceId}) - Total records: {valueResponse.Value.TotalRecords}, Imported records: {valueResponse.Value.ImportedRecords}, Records with errors: {valueResponse.Value.ErroredRecords}");
				}
			}
		}
	}
}
/* Expected console result:
	Import job state: Completed
	Import data source progress (sourceID: 412d8422-7b12-4cb1-9390-9f55858f98c9) - Total records: 4, Imported records: 4, Records with errors: 0
	Import data source progress (sourceID: 2819f6d0-2ec0-470c-aa02-d96b3ee082a1) - Total records: 2, Imported records: 2, Records with errors: 0
*/