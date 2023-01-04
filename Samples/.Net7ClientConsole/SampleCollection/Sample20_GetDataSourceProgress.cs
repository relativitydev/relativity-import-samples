// <copyright file="Sample20_GetDataSourceProgress.cs" company="Relativity ODA LLC">
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
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using Relativity.Import.Samples.Net7Client.Helpers;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of simple import native files.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample20_GetDataSourceProgress()
		{
			Console.WriteLine($"Running {nameof(Sample20_GetDataSourceProgress)}");
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			// set of columns indexes in load file used in import settings.
			const int controlNumberColumnIndex = 0;
			const int custodianColumnIndex = 1;
			const int dateSentColumnIndex = 5;
			const int emailToColumnIndex = 11;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Path to the load file used in data source settings.
			const string loadFile01Path = "C:\\DefaultFileRepository\\samples\\load_file_01.dat";

			// Create payload for request.
			var createJobPayload = new
			{
				applicationName = "Import-service-sample-app",
				correlationID = "Sample-job-0020"
			};

			// Configuration settings for document import. Builder is used to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number")
					.WithField(custodianColumnIndex, "Custodian - Single Choice")
					.WithField(emailToColumnIndex, "Email To")
					.WithField(dateSentColumnIndex, "Date Sent"))
				.WithoutFolders();

			// Create payload for request.
			var importSettingPayload = new { importSettings };

			// Configuration settings for data source. Builder is used to create settings.
			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
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

			// Create payload for request.
			var dataSourceSettingsPayload = new { dataSourceSettings };

			HttpClient httpClient = HttpClientHelper.CreateHttpClient();

			// Create import job.
			// endpoint: POST /import-jobs/{importId}
			var createImportJobUri = RelativityImportEndpoints.GetCreateImportUri(workspaceId, importId);

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
			var beginImportJobUri = RelativityImportEndpoints.GetBeginJobUri(workspaceId, importId);
			response = await httpClient.PostAsync(beginImportJobUri, null);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// End import job.
			// endpoint: POST /import-jobs/{importId}/end
			var endImportJobUri = RelativityImportEndpoints.GetEndJobUri(workspaceId, importId);
			response = await httpClient.PostAsync(endImportJobUri, null);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Get current import progress for specific data source.
			// endpoint: GET import-jobs/{importId}/sources/{sourceId}/progress"
			var importSourceProgressUri = RelativityImportEndpoints.GetImportSourceProgressUri(workspaceId, importId, sourceId);

			await ReadDataSourceProgress();

			// It may take some time for import job to be completed. Request data source details to monitor the current state.
			// endpoint: GET import-jobs/{importId}/sources/{sourceId}/details"
			var importSourceDetailsUri = RelativityImportEndpoints.GetImportSourceDetailsUri(workspaceId, importId, sourceId);

			JsonSerializerOptions options = new()
			{
				Converters = { new JsonStringEnumConverter() }
			};

			await ImportJobSampleHelper.WaitImportDataSourceToBeCompleted(
				funcAsync: () => httpClient.GetFromJsonAsync<ValueResponse<DataSourceDetails>> (importSourceDetailsUri, options),
				timeout: 10000);

			// Read data source progress.
			await ReadDataSourceProgress();

			async Task ReadDataSourceProgress()
			{
				var valueResponse = await httpClient.GetFromJsonAsync<ValueResponse<ImportProgress>>(importSourceProgressUri);

				if (valueResponse?.IsSuccess ?? false)
				{
					Console.WriteLine("\n");
					Console.WriteLine($"Import data source progress: Total records: {valueResponse.Value.TotalRecords}, Imported records: {valueResponse.Value.ImportedRecords}, Records with errors: {valueResponse.Value.ErroredRecords}");
				}
			}
		}
	}
}

/* Expected console result:

	Import data source progress: Total records: 0, Imported records: 0, Records with errors: 0

	DataSource state: Inserting
	DataSource state: Inserting
	DataSource state: Completed

	Import data source progress: Total records: 4, Imported records: 4, Records with errors: 0	
 */
