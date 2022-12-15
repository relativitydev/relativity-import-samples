// <copyright file="Sample01_ImportNativeFiles.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetClientConsole.SampleCollection
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Text;
    using System.Threading.Tasks;
    using Relativity.Import.V1;
    using Relativity.Import.V1.Builders.DataSource;
    using Relativity.Import.V1.Builders.Documents;
    using Relativity.Import.V1.Models.Settings;
    using Relativity.Import.V1.Models.Sources;
    using System.Net.Http.Json;
    using Relativity.Import.V1.Models;
    using Relativity.Import.Samples.NetClientConsole.HttpClientHelpers;
	using Relativity.Import.Samples.NetClientConsole.RestClientHelpers;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public class ImportServiceSample
	{
		/// <summary>
		/// Example of simple import native files.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample01_ImportNativeFiles()
		{
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1019056;

			// set of columns indexes in load file used in import settings.
			const int controlNumberColumnIndex = 0;
			const int custodianColumnIndex = 1;
			const int dateSentColumnIndex = 5;
			const int emailToColumnIndex = 11;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Path to the load file used in data source settings.
			const string loadFile01Path = "C:\\DefaultFileRepository\\samples\\load_file_01.dat";

			var jobInfoModel = new
			{
				applicationName = "import demo",
				correlationID = "correlation_id"
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

			// Wrap model into additional class .
			var importSettingParameter = new { importSettings = importSettings };

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

			var dataSourceSettingsParameter = new { dataSourceSettings = dataSourceSettings };


			HttpClient httpClient = HttpClientHelper.CreateHttpClient();
			
			var importSourceDetailsUri = $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/details";
			var importSourceProgressUri = $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/progress";

			// Create import job.
			var createImportJobUri = $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}";
			var response = await httpClient.PostAsJsonAsync(createImportJobUri, jobInfoModel);
			await ImportJobSampleHeper.EnsureSuccessResponse(response);

			// Add import document settings to existing import job (configure import job).
			var documentConfigurationUri = $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/documents-configurations";
			response = await httpClient.PostAsJsonAsync(documentConfigurationUri, importSettingParameter);
			await ImportJobSampleHeper.EnsureSuccessResponse(response);

			// Add data source settings to existing import job.
			var importSourcesUri = $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}";
			response = await httpClient.PostAsJsonAsync(importSourcesUri, dataSourceSettingsParameter);
			await ImportJobSampleHeper.EnsureSuccessResponse(response);

			// Start import job.
			var beginImportJobUri = $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/begin";
			response = await httpClient.PostAsync(beginImportJobUri, null);
			await ImportJobSampleHeper.EnsureSuccessResponse(response);

			// It may take some time for import job to be completed. Request data source details to monitor the current state.

			var dataSourceState = await ImportJobSampleHeper.WaitImportDataSourceToBeCompleted(
				funcAsync: () => httpClient.GetFromJsonAsync<ResponseWrapper<DataSourceDetails>> (importSourceDetailsUri),
				timeout: 10000);

			// Get current import progress for specific data source.

			var getResponses = await httpClient.GetStringAsync(importSourceProgressUri);
			
			var valueResponse = await httpClient.GetFromJsonAsync<ValueResponse<ImportProgress>>(importSourceProgressUri);
			
			if (valueResponse?.IsSuccess ?? false)
			{
				Console.WriteLine($"\n Data source state: {dataSourceState}");
				Console.WriteLine($"Import progress: Total records: {valueResponse.Value.TotalRecords}, Imported records: {valueResponse.Value.ImportedRecords}, Records with errors: {valueResponse.Value.ErroredRecords}");
			}

			// End import job.
			var endImportJobUri = $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/end";
			response = await httpClient.PostAsync(endImportJobUri, null);
			await ImportJobSampleHeper.EnsureSuccessResponse(response);
			}
	}
}
