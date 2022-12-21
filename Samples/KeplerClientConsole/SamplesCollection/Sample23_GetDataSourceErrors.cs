// <copyright file="Sample23_GetDataSourceErrors.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
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
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId01 = Guid.NewGuid();
			Guid sourceId02 = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1031725;

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
					correlationID: "Sample-job-00020");

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add import document settings to existing import job.
					response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add first data source settings to existing import job.
					response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId01, dataSourceSettings01);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add second data source settings to existing import job.
					response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId02, dataSourceSettings02);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Start import job.
					response = await importJobController.BeginAsync(workspaceId, importId);
				}

				// End import job - assuming no new data source won't be added later.
				await importJobController.EndAsync(workspaceId, importId);

				// It takes some time for import job to be completed. Request import job details to monitor its final state.
				// remark: this sample based on checking import completed/failed because job was already ended.
				var importJobState = await this.WaitImportJobToBeCompleted(
					funcAsync: () => importJobController.GetDetailsAsync(workspaceId, importId));

				ValueResponse<ImportProgress> importProgress = await importJobController.GetProgressAsync(workspaceId, importId);

				Console.WriteLine($"\n\nImport Job State: {importJobState}");
				Console.WriteLine($"Import progress: Total records: {importProgress.Value.TotalRecords}, Imported records: {importProgress.Value.ImportedRecords}, Records with errors: {importProgress.Value.ErroredRecords}");

				foreach (var source in new[] { sourceId01, sourceId02 })
				{
					// Get current import progress for specific data source.
					var sourceDetails = await importSourceController.GetDetailsAsync(workspaceId, importId, source);

					Console.WriteLine($"Data Source Id: {source} finished with state {sourceDetails.Value.State}");
					if (sourceDetails.Value.State == DataSourceState.Failed)
					{
						Console.WriteLine("Data source failed due to errors:");
						foreach (var importError in sourceDetails.Value.JobLevelErrors)
						{
							// Retrieve all error details.
							foreach (var details in importError.ErrorDetails)
							{
								Console.WriteLine($"Line Number: {importError.LineNumber},	ColumnIndex: {details.ColumnIndex}, ErrorCode: {details.ErrorCode} ErrorMessage: {details.ErrorMessage} ");
							}
						}
					}
					else if (sourceDetails.Value.State == DataSourceState.CompletedWithItemErrors)
					{
						Console.WriteLine("Data source completed with item errors:");

						// Get Item Errors for each source.
						ValueResponse<ImportErrors> dataSourceErrors =
							await importSourceController.GetItemErrorsAsync(workspaceId, importId, source, 0, 20);

						foreach (var importError in dataSourceErrors.Value.Errors)
						{
							// Retrieve all error details for each line.
							foreach (var details in importError.ErrorDetails)
							{
								Console.WriteLine(
									$"Line Number: {importError.LineNumber},	ColumnIndex: {details.ColumnIndex}, ErrorCode: {details.ErrorCode} ErrorMessage: {details.ErrorMessage} ");
							}
						}

						Console.WriteLine($"Number of records: {dataSourceErrors.Value.NumberOfRecords}");
						Console.WriteLine($"Total count: {dataSourceErrors.Value.TotalCount}");
						Console.WriteLine($"Number of skipped records: {dataSourceErrors.Value.NumberOfSkippedRecords}");
						Console.WriteLine($"Has more records: {dataSourceErrors.Value.HasMoreRecords}");
					}
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
