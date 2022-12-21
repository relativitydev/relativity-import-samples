// <copyright file="Sample19_GetImportJobProgress.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.DataSource;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading all data sources Ids for particular job.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample19_GetImportJobProgress()
		{
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

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
					.WithField(custodianColumnIndex, "Custodian - Single Choice")
					.WithField(emailToColumnIndex, "Email To")
					.WithField(dateSentColumnIndex, "Date Sent"))
				.WithoutFolders();

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

			using (Relativity.Import.V1.Services.IDocumentConfigurationController documentConfiguration =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IDocumentConfigurationController>())

			using (Relativity.Import.V1.Services.IImportJobController importJobController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())

			using (Relativity.Import.V1.Services.IImportSourceController importSourceController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportSourceController>())
			{
				// Create Job
				Response response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: "Sample-job-0019");

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Configure Job.
					response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add data source settings to existing import job.
					response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId, dataSourceSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Start job.
					response = await importJobController.BeginAsync(workspaceId, importId);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// It may take some time for import job to be completed. Request data source details to monitor the current state.
					var dataSourceState = await this.WaitToStatusChange(
						targetStatus: DataSourceState.Completed,
						funcAsync: () => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId),
						timeout: 10000);

					// Get current import details of specific job.
					ValueResponse<ImportDetails> importDetails =
						await importJobController.GetDetailsAsync(workspaceId, importId);

					Console.WriteLine(
						$"State: {importDetails.Value.State} Application Name: {importDetails.Value.ApplicationName}");

					// Get current import progress for provided job.
					ValueResponse<ImportProgress> importProgress =
						await importJobController.GetProgressAsync(workspaceId, importId);

					Console.WriteLine(
						$"Import progress- Total: {importProgress.Value.TotalRecords}  Imported: {importProgress.Value.ImportedRecords}  WithErrors: {importProgress.Value.ErroredRecords}");

					await importJobController.EndAsync(workspaceId, importId);
				}
			}
		}
	}
}
