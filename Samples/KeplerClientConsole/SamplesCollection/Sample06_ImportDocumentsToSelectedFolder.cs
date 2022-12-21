// <copyright file="Sample06_ImportDocumentsToSelectedFolder.cs" company="Relativity ODA LLC">
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

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of settings used to import documents to selected folder under the workspace.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample06_ImportDocumentsToSelectedFolder()
		{
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1031725;

			// destination folder artifact id.
			const int rootFolderId = 1041269;

			// set of columns indexes in load file used in import settings.
			const int folderPathColumnIndex = 15;
			const int controlNumberColumnIndex = 0;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;
			const string loadFile01Path = "C:\\DefaultFileRepository\\samples\\load_file_03.dat";

			// Configuration settings for document import. Builder is used to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number"))
				.WithFolders(f => f
					.WithRootFolderID(rootFolderId, r => r
						.WithFolderPathDefinedInColumn(folderPathColumnIndex)));

			// Configuration settings for data source. Builder is used to create settings with default delimiters.
			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForLoadFile(loadFile01Path)
				.WithDefaultDelimiters()
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
					correlationID: "Sample-job-0006");

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add import document settings to existing import job.
					response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add data source settings to existing import job.
					response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId, dataSourceSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Start import job.
					response = await importJobController.BeginAsync(workspaceId, importId);
				}

				if (!this.IsPreviousResponseWithSuccess(response))
				{
					Console.WriteLine($"Import Job was not started because of:  {response.ErrorCode} - {response.ErrorMessage}");
					return;
				}

				// It may take some time for import job to be completed. Request data source details to monitor the current state.
				var dataSourceState = await this.WaitToStatusChange(
					targetStatus: DataSourceState.Completed,
					funcAsync: () => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId),
					timeout: 10000);

				// Get current import progress for specific data source.
				var importProgress = await importSourceController.GetProgressAsync(workspaceId, importId, sourceId);

				if (importProgress.IsSuccess)
				{
					Console.WriteLine($"\nData source state: {dataSourceState}");
					Console.WriteLine($"Import progress: Total records: {importProgress.Value.TotalRecords}, Imported records: {importProgress.Value.ImportedRecords}, Records with errors: {importProgress.Value.ErroredRecords}");
				}

				// End import job.
				await importJobController.EndAsync(workspaceId, importId);
			}
		}
	}
}

// Expected output for sample load file.
// Data source state: Completed
// Import progress: Total records: 2, Imported records: 2, Records with errors: 0