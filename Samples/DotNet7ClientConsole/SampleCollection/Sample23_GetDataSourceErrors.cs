// <copyright file="Sample23_GetDataSourceErrors.cs" company="Relativity ODA LLC">
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
	using Relativity.Import.V1.Models.Errors;
	using System.Collections.Generic;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading data source errors.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample23_GetDataSourceErrors()
		{
			Console.WriteLine($"Running {nameof(Sample23_GetDataSourceErrors)}");

			// GUID identifiers for import job and data sources.
			Guid importId = Guid.NewGuid();
			Guid source01Id = Guid.NewGuid();
			Guid source02Id = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			// set of columns indexes in load file used in import settings.
			// set of columns indexes in load file used in import settings.
			const int controlNumberColumnIndex = 0;
			const int custodianColumnIndex = 1;
			const int dateSentColumnIndex = 5;
			const int emailToColumnIndex = 11;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Path to the load file used in data source settings.
			// first file contains incorrect values.
			const string loadFile01Path = "C:\\DefaultFileRepository\\samples\\load_file_05.dat";
			const string loadFile02Path = "C:\\DefaultFileRepository\\samples\\notExistingFile.dat";

			// Create payload for request.
			var createJobPayload = new
			{
				applicationName = "Import-service-sample-app",
				correlationID = "Sample-job-0023"
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
			var importSettingPayload = new {importSettings};

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
			var createImportJobUri = RelativityImportEndpoints.GetImportJobCreateUri(workspaceId, importId);
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

			// Add second data source settings to existing import job.
			importSourcesUri = RelativityImportEndpoints.GetImportSourceUri(workspaceId, importId, source02Id);
			response = await httpClient.PostAsJsonAsync(importSourcesUri, dataSourceSettingsPayload02);
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

			// It may take some time for import job to be completed. Request job details to monitor the current state.
			// You can also get data source details to verify if importing source is finished.
			var importDetailsUri = RelativityImportEndpoints.GetImportJobDetailsUri(workspaceId, importId);

			JsonSerializerOptions options = new()
			{
				Converters = { new JsonStringEnumConverter() }
			};

			await ImportJobSampleHelper.WaitImportJobToBeFinished(
				funcAsync: () => httpClient.GetFromJsonAsync<ValueResponse<ImportDetails>>(importDetailsUri, options));


			// Get import Progress (see sample 19)
			var importJobProgressUri = RelativityImportEndpoints.GetImportJobProgressUri(workspaceId, importId);

			var valueResponse = await httpClient.GetFromJsonAsync<ValueResponse<ImportProgress>>(importJobProgressUri);

			if (valueResponse?.IsSuccess ?? false)
			{
				Console.WriteLine("\n");
				Console.WriteLine($"IsSuccess: {valueResponse.IsSuccess}");
				Console.WriteLine($"Import job Id: {valueResponse.ImportJobID}");
				Console.WriteLine($"Import job progress: Total records: {valueResponse.Value.TotalRecords}, Imported records: {valueResponse.Value.ImportedRecords}, Records with errors: {valueResponse.Value.ErroredRecords}");
			}


			foreach (var sourceId in new[] {source01Id, source02Id})
			{
				// Get current import progress for specific data source.
				// endpoint: GET import-jobs/{importId}/sources/{sourceId}/progress"

				var importSourceDetailsUri = RelativityImportEndpoints.GetImportSourceDetailsUri(workspaceId, importId,sourceId);
				var sourceDetails = await httpClient.GetFromJsonAsync<ValueResponse<DataSourceDetails>>(importSourceDetailsUri, options);

				Console.WriteLine($"Data Source Id: {sourceId} finished with state {sourceDetails?.Value.State}");
				switch (sourceDetails?.Value.State)
				{
					case DataSourceState.Failed:
						PrintJobLevelErrors(sourceDetails.Value.JobLevelErrors);
						break;
					case DataSourceState.CompletedWithItemErrors:
					{
						Console.WriteLine("Data source completed with item errors:");

						// Get Item Errors for each source.
						// GET import-jobs/{importId}/sources/{sourceId}/itemerrors?{start}&{length}"
						var getItemErrorUrl = RelativityImportEndpoints.GetImportSourceItemErrorsUri(workspaceId, importId, sourceId,0 , 20);
						ValueResponse<ImportErrors>? valueResponseErrors = await httpClient.GetFromJsonAsync<ValueResponse<ImportErrors>>(getItemErrorUrl);

						if (valueResponseErrors is {IsSuccess: true})
						{
							PrintItemLevelErrors(valueResponseErrors.Value);
						}

						break;
					}
				}
			}
		}

		private static void PrintItemLevelErrors(ImportErrors importErrors)
		{
			foreach (var importError in importErrors.Errors)
			{
				// Retrieve all error details for each line.
				foreach (var details in importError.ErrorDetails)
				{
					Console.WriteLine(
						$"Line Number: {importError.LineNumber},	ColumnIndex: {details.ColumnIndex}, ErrorCode: {details.ErrorCode} ErrorMessage: {details.ErrorMessage} ");
				}
			}

			Console.WriteLine($"Number of records: {importErrors.NumberOfRecords}");
			Console.WriteLine($"Total count: {importErrors.TotalCount}");
			Console.WriteLine($"Number of skipped records: {importErrors.NumberOfSkippedRecords}");
			Console.WriteLine($"Has more records: {importErrors.HasMoreRecords}");
		}

		private static void PrintJobLevelErrors(List<ImportError> jobLevelErrors)
		{
			Console.WriteLine("Data source failed due to errors:");
			foreach (var importError in jobLevelErrors)
			{
				// Retrieve all error details.
				foreach (var details in importError.ErrorDetails)
				{
					Console.WriteLine($"Line Number: {importError.LineNumber},	ColumnIndex: {details.ColumnIndex}, ErrorCode: {details.ErrorCode} ErrorMessage: {details.ErrorMessage} ");
				}
			}
		}
	}
}
// Expected console output for sample data source files.

// Import Job State: Failed
// Import progress: Total records: 4, Imported records: 0, Records with errors: 4

// Data Source Id: cb733792-2a70-41f7-84b9-3ee821595097 finished with state CompletedWithItemErrors
// Data source completed with item errors:
//    Line Number: 2, ColumnIndex: 0, ErrorCode: S.LN.INT.4015 ErrorMessage: Error in row 1, field "date sent".Invalid date.
//    Line Number: 3, ColumnIndex: 0, ErrorCode: S.LN.INT.4015 ErrorMessage: Error in row 2, field "date sent".Invalid date.
//    Line Number: 4, ColumnIndex: 0, ErrorCode: S.LN.INT.4101 ErrorMessage: An item with identifier Sample_0000003 already exists in the workspace
//    Line Number: 5, ColumnIndex: 0, ErrorCode: S.LN.INT.4101 ErrorMessage: An item with identifier Sample_0000004 already exists in the workspace
// Number of records: 4
// Total count: 4
// Number of skipped records: 0
// Has more records: False

// Data Source Id: 0f3c8860-5fe6-414d-9bae-1e0f628957c3 finished with state Failed
// Data source failed due to errors:
//    Line Number: -1, ColumnIndex: -1, ErrorCode: S.RD.EXT.0217 ErrorMessage: Cannot read Data Source. Could not open file for reading by RestartableStream.
//    Line Number: -1, ColumnIndex: -1, ErrorCode: J.RUN.EXT.0217 ErrorMessage: Cannot run import job. Could not open file for reading by RestartableStream.
