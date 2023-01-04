// <copyright file="Sample11_ImportDocumentSettingsForImages.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
    using Relativity.Import.V1;
	using Relativity.Import.V1.Models;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of creating ImportDocumentSettings for image import manually - without using ImportDocumentSettingsBuilder.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample11_ImportDocumentSettingsForImages()
		{
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace and root folder artifact Ids.
			const int workspaceId = 1031725;
			const int rootFolderId = 1042316;

			// Example of configuration settings for images import created without ImportDocumentSettingsBuilder.
			ImportDocumentSettings importSettings = new ImportDocumentSettings()
			{
				Overlay = new OverlaySettings
				{
					Mode = OverlayMode.AppendOverlay,
					KeyField = default,
					MultiFieldOverlayBehaviour = MultiFieldOverlayBehaviour.UseRelativityDefaults,
				},
				Native = null,
				Image = new ImageSettings
				{
					PageNumbering = PageNumbering.AutoNumberImages,
					ProductionID = null,
					LoadExtractedText = true,
				},
				Fields = null,
				Folder = new FolderSettings
				{
					FolderPathColumnIndex = null,
					RootFolderID = rootFolderId,
				},
				Other = new OtherSettings
				{
					ExtractedText = new ExtractedTextSettings
					{
						ValidateEncoding = true,
					},
				},
			};

			// Configuration settings for data source created without DataSourceSettingsBuilder.
			DataSourceSettings dataSourceSettings = new DataSourceSettings
			{
				Type = DataSourceType.Opticon,
				Path = "C:\\DefaultFileRepository\\samples\\opticon_01.opt",
				NewLineDelimiter = default,
				ColumnDelimiter = default,
				QuoteDelimiter = default,
				MultiValueDelimiter = default,
				NestedValueDelimiter = default,
				Encoding = null,
				CultureInfo = "en-us",
				EndOfLine = DataSourceEndOfLine.Windows,
				FirstLineContainsColumnNames = false,
				StartLine = 0,
			};

			using (Relativity.Import.V1.Services.IDocumentConfigurationController documentConfiguration = this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IDocumentConfigurationController>())

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
					correlationID: "Sample-job-00011-settings");

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add import images configuration to existing import job.
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

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// It may take some time for import job to be completed. Request data source details to monitor the current state.
					var dataSourceState = await this.WaitToStatusChange(
						targetStatus: DataSourceState.Completed,
						funcAsync: () => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId),
						timeout: 10000);

					// Get current import progress for specific data source.
					ValueResponse<ImportProgress> importProgress = await importJobController.GetProgressAsync(workspaceId, importId);

					if (importProgress.IsSuccess)
					{
						Console.WriteLine($"\nData source state: {dataSourceState}");
						Console.WriteLine($"Import progress: Total records: {importProgress.Value.TotalRecords}, Imported records: {importProgress.Value.ImportedRecords}, Records with errors: {importProgress.Value.ErroredRecords}");
					}
				}
			}
		}
	}
}
