// <copyright file="Sample03_ImportFromTwoDataSources.cs" company="Relativity ODA LLC">
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
			Console.WriteLine($"Running {nameof(Sample03_ImportFromTwoDataSources)}");

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

			using (IDocumentConfigurationController documentConfiguration = this._serviceFactory.CreateProxy<IDocumentConfigurationController>())
			using (IImportJobController importJobController = this._serviceFactory.CreateProxy<IImportJobController>())
			using (IImportSourceController importSourceController = this._serviceFactory.CreateProxy<IImportSourceController>())
			{
				// Create import job.
				Response response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: "Sample-job-0003");

				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CreateAsync");

				// Add import document settings to existing import job.
				response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				ResponseHelper.EnsureSuccessResponse(response, "IDocumentConfigurationController.CreateAsync");

				// Add data source settings to existing import job.
				response = await importSourceController.AddSourceAsync(workspaceId, importId, source01Id, dataSourceSettings01);
				ResponseHelper.EnsureSuccessResponse(response, "IImportSourceController.AddSourceAsync");

				// Add second data source settings to existing import job.
				// Both data sources are added before job starts.
				// Data source can be also added when job is running.
				response = await importSourceController.AddSourceAsync(workspaceId, importId, source02Id, dataSourceSettings02);
				ResponseHelper.EnsureSuccessResponse(response, "IImportSourceController.AddSourceAsync");

				// Start import job.
				response = await importJobController.BeginAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.BeginAsync");

				// End import job.
				await importJobController.EndAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.EndAsync");

				// It may take some time for import job to be completed. Request import job details to monitor the current state.
				// You can also get data sources details to verify if import sources are finished.
				var importJobState = await this.WaitImportJobToBeCompleted(
					funcAsync: () => importJobController.GetDetailsAsync(workspaceId, importId),
					timeout: 10000);

				Console.WriteLine($"Import job state: {importJobState}");

				foreach (var sourceId in new[] { source01Id, source02Id })
				{
					// Get current import progress for specific data source.
					var valueResponse = await importSourceController.GetProgressAsync(workspaceId, importId, sourceId);

					if (valueResponse.IsSuccess)
					{
						Console.WriteLine($"Import data source progress (sourceID: {sourceId}) - Total records: {valueResponse.Value.TotalRecords}, Imported records: {valueResponse.Value.ImportedRecords}, Records with errors: {valueResponse.Value.ErroredRecords}");
					}
				}
			}
		}
	}
}

/* Expected console result:

	Import job state: Completed
	Import data source progress (sourceID: 765f398f-66f2-4cb0-b89d-7f597ecf7ecb) - Total records: 4, Imported records: 4, Records with errors: 0
	Import data source progress (sourceID: ee95d176-f648-4365-a51e-f24810ef1b0f) - Total records: 2, Imported records: 2, Records with errors: 0
*/