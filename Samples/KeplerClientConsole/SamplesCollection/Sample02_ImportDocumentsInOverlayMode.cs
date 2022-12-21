// <copyright file="Sample02_ImportDocumentsInOverlayMode.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;

	using Relativity.Import.Samples.NetFrameworkClient.ImportSampleHelpers;
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
		/// Example of configuring import job to import documents in overlay mode.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample02_ImportDocumentsInOverlayMode()
		{
			Console.WriteLine($"Running {nameof(Sample02_ImportDocumentsInOverlayMode)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id
			const int workspaceId = 1019056;

			// set of columns indexes in load file used in import settings
			const int controlNumberColumnIndex = 0;
			const int custodianColumnIndex = 1;
			const int dateSentColumnIndex = 5;
			const int dateReceivedColumnIndex = 5;
			const int emailToColumnIndex = 11;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Path to the load file used in data source settings.
			const string loadFile01Path = "C:\\DefaultFileRepository\\samples\\load_file_01.dat";

			// overlay key field
			const string overlayKeyField = "Control Number";

			// Configuration settings for document import. Use builder to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithOverlayMode(x => x
					.WithKeyField(overlayKeyField)
					.WithMultiFieldOverlayBehaviour(MultiFieldOverlayBehaviour.MergeAll))
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number")
					.WithField(custodianColumnIndex, "Custodian - Single Choice")
					.WithField(emailToColumnIndex, "Email To")
					.WithField(dateSentColumnIndex, "Date Sent")
					.WithField(dateReceivedColumnIndex, "Date Received"))
				.WithoutFolders();

			// Configuration settings for data source with custom delimiters. Use builder to create settings.
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
				// Create import job.
				Response response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: "Sample-job-0002");

				ResponseHelper.EnsureSuccessResponse(response, nameof(IImportJobController.CreateAsync));

				// Add import document settings to existing import job.
				response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				ResponseHelper.EnsureSuccessResponse(response, nameof(IDocumentConfigurationController.CreateAsync));


				// Add data source settings to existing import job.
				response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId, dataSourceSettings);
				ResponseHelper.EnsureSuccessResponse(response, nameof(IImportSourceController.AddSourceAsync));


				// Start import job.
				response = await importJobController.BeginAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, nameof(IImportJobController.BeginAsync));


				// End import job.
				await importJobController.EndAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, nameof(IImportJobController.EndAsync));


				// It may take some time for import job to be completed. Request data source details to monitor the current state.
				var dataSourceState = await this.WaitImportDataSourceToBeCompleted(
					funcAsync: () => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId),
					timeout: 10000);

				// Get current import progress for specific data source.
				var importProgress = await importSourceController.GetProgressAsync(workspaceId, importId, sourceId);

				ResponseHelper.EnsureSuccessResponse(importProgress, nameof(IImportSourceController.GetProgressAsync));


				if (importProgress.IsSuccess)
				{
					Console.WriteLine("\n");
					Console.WriteLine($"Data source state: {dataSourceState}");
					Console.WriteLine($"Import progress: Total records: {importProgress.Value.TotalRecords}, Imported records: {importProgress.Value.ImportedRecords}, Records with errors: {importProgress.Value.ErroredRecords}");
				}
				Console.WriteLine("\n");
			}
		}
	}
}
