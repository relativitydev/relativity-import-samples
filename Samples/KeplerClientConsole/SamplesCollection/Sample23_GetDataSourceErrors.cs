// <copyright file="Sample23_GetDataSourceErrors.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetFrameworkClient.SamplesCollection
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Relativity.Import.Samples.DotNetFrameworkClient.ImportSampleHelpers;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.DataSource;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models;
	using Relativity.Import.V1.Models.Errors;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading data sources errors.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample23_GetDataSourceErrors()
		{
			Console.WriteLine($"Running {nameof(Sample23_GetDataSourceErrors)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId01 = Guid.NewGuid();
			Guid sourceId02 = Guid.NewGuid();

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
			// first file contains incorrect values.
			const string loadFile01Path = "\\files\\<TenantNumber>\\Files\\SampleDataSources\\load_file_05.dat";
			const string loadFile02Path = "\\files\\<TenantNumber>\\Files\\SampleDataSources\\notExistingFile.dat";

			// Configuration settings for data source. Builder is used to create settings.
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

			// Configuration settings for second data source
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

			using (Relativity.Import.V1.Services.IDocumentConfigurationController documentConfiguration =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IDocumentConfigurationController>())

			using (Relativity.Import.V1.Services.IImportJobController importJobController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())

			using (Relativity.Import.V1.Services.IImportSourceController importSourceController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportSourceController>())
			{
				// Create import job.
				Response response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: "Sample-job-00023");

				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CreateAsync");

				// Add import document settings to existing import job.
				response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				ResponseHelper.EnsureSuccessResponse(response, "IDocumentConfigurationController.CreateAsync");

				// Add first data source settings to existing import job.
				response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId01, dataSourceSettings01);
				ResponseHelper.EnsureSuccessResponse(response, "IImportSourceController.AddSourceAsync");

				// Add second data source settings to existing import job.
				response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId02, dataSourceSettings02);
				ResponseHelper.EnsureSuccessResponse(response, "IImportSourceController.AddSourceAsync");

				// Start import job.
				response = await importJobController.BeginAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.BeginAsync");

				// End import job - assuming no new data source won't be added later.
				await importJobController.EndAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.EndAsync");

				// It takes some time for import job to be completed. Request import job details to monitor its final state.
				// remark: this sample based on checking import completed/failed because job was already ended.
				var importJobState = await this.WaitImportJobToBeCompleted(
					funcAsync: () => importJobController.GetDetailsAsync(workspaceId, importId));

				Console.WriteLine($"Import job state: {importJobState}");
				ValueResponse <ImportProgress> valueResponse = await importJobController.GetProgressAsync(workspaceId, importId);

				if (valueResponse?.IsSuccess ?? false)
				{
					Console.WriteLine("\n");
					Console.WriteLine($"IsSuccess: {valueResponse.IsSuccess}");
					Console.WriteLine($"Import job Id: {valueResponse.ImportJobID}");
					Console.WriteLine($"Import job progress: Total records: {valueResponse.Value.TotalRecords}, Imported records: {valueResponse.Value.ImportedRecords}, Records with errors: {valueResponse.Value.ErroredRecords}");
				}


				foreach (var source in new[] { sourceId01, sourceId02 })
				{
					// Get current import progress for specific data source.
					var sourceDetails = await importSourceController.GetDetailsAsync(workspaceId, importId, source);

					Console.WriteLine($"Data Source Id: {source} finished with state {sourceDetails.Value.State}");

					if (sourceDetails.Value.State == DataSourceState.Failed)
					{
						PrintJobLevelErrors(sourceDetails.Value.JobLevelErrors);
					}
					else if (sourceDetails.Value.State == DataSourceState.CompletedWithItemErrors)
					{
						Console.WriteLine("Data source completed with item errors:");

						// Get Item Errors for each source.
						ValueResponse<ImportErrors> dataSourceErrors =
							await importSourceController.GetItemErrorsAsync(workspaceId, importId, source, 0, 20);

						PrintItemLevelErrors(dataSourceErrors.Value);
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
						$"Line Number: {importError.LineNumber}, ColumnIndex: {details.ColumnIndex}, ErrorCode: {details.ErrorCode} ErrorMessage: {details.ErrorMessage} ");
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
					Console.WriteLine($"Line Number: {importError.LineNumber}, ColumnIndex: {details.ColumnIndex}, ErrorCode: {details.ErrorCode} ErrorMessage: {details.ErrorMessage} ");
				}
			}
		}
	}
}

/* Expected console output

// Import Job State: Failed
 Import progress: Total records: 4, Imported records: 0, Records with errors: 4

 Data Source Id: cb733792-2a70-41f7-84b9-3ee821595097 finished with state CompletedWithItemErrors
 Data source completed with item errors:
    Line Number: 2, ColumnIndex: 0, ErrorCode: S.LN.INT.4015 ErrorMessage: Error in row 1, field "date sent".Invalid date.
    Line Number: 3, ColumnIndex: 0, ErrorCode: S.LN.INT.4015 ErrorMessage: Error in row 2, field "date sent".Invalid date.
    Line Number: 4, ColumnIndex: 0, ErrorCode: S.LN.INT.4101 ErrorMessage: An item with identifier Sample_0000003 already exists in the workspace
    Line Number: 5, ColumnIndex: 0, ErrorCode: S.LN.INT.4101 ErrorMessage: An item with identifier Sample_0000004 already exists in the workspace
 Number of records: 4
 Total count: 4
 Number of skipped records: 0
 Has more records: False

 Data Source Id: 0f3c8860-5fe6-414d-9bae-1e0f628957c3 finished with state Failed
 Data source failed due to errors:
    Line Number: -1, ColumnIndex: -1, ErrorCode: S.RD.EXT.0217 ErrorMessage: Cannot read Data Source. Could not open file for reading by RestartableStream.
    Line Number: -1, ColumnIndex: -1, ErrorCode: J.RUN.EXT.0217 ErrorMessage: Cannot run import job. Could not open file for reading by RestartableStream.
*/