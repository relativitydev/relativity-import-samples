// <copyright file="Sample05_ImportDocumentsWithExtractedText.cs" company="Relativity ODA LLC">
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
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of configure import documents with extracted text located in separated file.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample05_ImportDocumentsWithExtractedText()
		{
			Console.WriteLine($"Running {nameof(Sample05_ImportDocumentsWithExtractedText)}");

			// GUID identifiers for import job and data sources.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// Destination workspace artifact Id
			const int workspaceId = 1000000;

			// set of columns indexes in load file used in import settings.
			const int controlNumberColumnIndex = 0;
			const int extractedTextPathColumnIndex = 12;
			const int fileNameColumnIndex = 13;
			const int fileSizeColumnIndex = 14;
			const int filePathColumnIndex = 22;

			// Path to the load files used in data source settings.
			const string loadFile01Path = "C:\\DefaultFileRepository\\samples\\load_file_01.dat";

			// Configuration settings for document import. Builder is used to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number")
					.WithExtractedTextField(extractedTextPathColumnIndex, e => e
						.WithExtractedTextInSeparateFiles(f => f
							.WithEncoding("UTF-8")
							.WithFileSizeDefinedInColumn(fileSizeColumnIndex))))
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
					correlationID: "Sample-job-0005");

				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CreateAsync");

				// Add import document settings to existing import job.
				response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				ResponseHelper.EnsureSuccessResponse(response, "IDocumentConfigurationController.CreateAsync");

				// Add data source settings to existing import job.
				response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId, dataSourceSettings01);
				ResponseHelper.EnsureSuccessResponse(response, "IImportSourceController.AddSourceAsync");

				// Start import job.
				response = await importJobController.BeginAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.BeginAsync");

				// End import job.
				await importJobController.EndAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.EndAsync");

				// It may take some time for import job to be completed. Request data source details to monitor the current state.
				var dataSourceState = await this.WaitImportDataSourceToBeCompleted(
					funcAsync: () => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId),
					timeout: 10000);

				// Get current import progress for specific data source.
				var importProgress = await importSourceController.GetProgressAsync(workspaceId, importId, sourceId);

				if (importProgress.IsSuccess)
				{
					Console.WriteLine($"\nData source state: {dataSourceState}");
					Console.WriteLine($"Import data source progress: Total records: {importProgress.Value.TotalRecords}, Imported records: {importProgress.Value.ImportedRecords}, Records with errors: {importProgress.Value.ErroredRecords}");
				}
			}
		}
	}
}

/* Expected console result:
	Data source state: Completed
	Import data source progress: Total records: 4, Imported records: 4, Records with errors: 0
 */