// <copyright file="Sample03_ImportFromTwoDataSources.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.DataSource;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;
	using Relativity.Import.V1.Services;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of using two data sources to import documents from them.
		/// In this example both data sources are added before import job is started.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample03_ImportFromTwoDataSources()
		{
			// GUID identifiers for import job and data sources.
			Guid importId = Guid.NewGuid();
			Guid source01Id = Guid.NewGuid();
			Guid source02Id = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1035012;

			// set of columns indexes in load file used in import settings.
			const int controlNumberColumnIndex = 0;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Path to the load files used in data source settings.
			const string loadFile01Path = "C:\\DefaultFileRepository\\samples\\load_file_01.dat";
			const string loadFile02Path = "C:\\DefaultFileRepository\\samples\\load_file_02.dat";

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

			// Configuration settings for data source.  Builder is used to create settings.
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

			using (IDocumentConfigurationController documentConfiguration =
				this._serviceFactory.CreateProxy<IDocumentConfigurationController>())
			using (IImportJobController importJobController =
				   this._serviceFactory.CreateProxy<IImportJobController>())
			using (IImportSourceController importSourceController =
				   this._serviceFactory.CreateProxy<IImportSourceController>())
			{
				// Create import job.
				Response response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: "Sample-job-0003");

				if (this.IsPreviousResponseWithSuccess(response, nameof(IImportJobController.CreateAsync)))
				{
					// Add import document settings to existing import job.
					response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response, nameof(IDocumentConfigurationController.CreateAsync)))
				{
					// Add data source settings to existing import job.
					response = await importSourceController.AddSourceAsync(workspaceId, importId, source01Id, dataSourceSettings01);
				}

				if (this.IsPreviousResponseWithSuccess(response, nameof(IImportSourceController.AddSourceAsync)))
				{
					// Add second data source settings to existing import job.
					// Both data sources are added before job starts.
					// Data source can be also added when job is running.
					response = await importSourceController.AddSourceAsync(workspaceId, importId, source02Id, dataSourceSettings02);
				}

				if (this.IsPreviousResponseWithSuccess(response, nameof(IImportSourceController.AddSourceAsync)))
				{
					// Start import job.
					response = await importJobController.BeginAsync(workspaceId, importId);
				}

				if (!this.IsPreviousResponseWithSuccess(response, nameof(IImportJobController.BeginAsync)))
				{
					Console.WriteLine($"Import Job was not started because of:  {response.ErrorCode} - {response.ErrorMessage}");
					return;
				}

				foreach (var sourceId in new[] { source01Id, source02Id })
				{
					// It may take some time for import job to be completed. Request data source details to monitor the current state.
					var dataSourceState = await this.WaitToStatusChange(
						targetStatus: DataSourceState.Completed,
						funcAsync: () => importSourceController.GetDetailsAsync(workspaceId, importId, source01Id),
						timeout: 10000);

					// Get current import progress for specific data source.
					var importProgress = await importSourceController.GetProgressAsync(workspaceId, importId, sourceId);

					if (importProgress.IsSuccess)
					{
						Console.WriteLine($"Data source [{sourceId}] state: {dataSourceState}");
						Console.WriteLine($"Import progress: Total records: {importProgress.Value.TotalRecords}, Imported records: {importProgress.Value.ImportedRecords}, Records with errors: {importProgress.Value.ErroredRecords}");
					}
				}

				// End import job.
				await importJobController.EndAsync(workspaceId, importId);
			}
		}
	}
}

// Example console output for sample files
// Data source [69537ef7-7c9d-41d8-a8bc-32c641db14d1] state: Completed
// Import progress: Total records: 4, Imported records: 4, Records with errors: 2
// DataSource status: Completed
// Data source [3c461422-3785-44a8-874f-536e2610c8e9] state: Completed
// Import progress: Total records: 2, Imported records: 2, Records with errors: 0