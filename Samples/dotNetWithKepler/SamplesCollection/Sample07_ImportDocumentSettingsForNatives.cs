// <copyright file="Sample07_ImportDocumentSettingsForNatives.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of setting ImportDocumentSetting manually - without ImportDocumentSettingsBuilder.
		/// Settings to be used to import images.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample07_ImportDocumentSettingsForNatives()
		{
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace and folder artifact Ids.
			const int workspaceId = 1031725;
			const int rootFolderId = 1042316;

			// overlay keyField
			const string keyField = "Control Number";

			// set of columns indexes in load file used in import settings.
			const int extractedTextFilePathColumnIndex = 12;
			const int emailToColumnIndex = 11;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Configuration settings for document import. Example of set without using ImportDocumentSettingsBuilder.
			ImportDocumentSettings importSettings = new ImportDocumentSettings()
			{
				Overlay = new OverlaySettings
				{
					Mode = OverlayMode.AppendOverlay,
					KeyField = keyField,
					MultiFieldOverlayBehaviour = MultiFieldOverlayBehaviour.UseRelativityDefaults,
				},
				Native = new NativeSettings
				{
					FileNameColumnIndex = fileNameColumnIndex,
					FilePathColumnIndex = filePathColumnIndex,
				},
				Fields = new FieldsSettings
				{
					FieldMappings = new[]
					{
						new FieldMapping
						{
							Field = "Control Number",
							ContainsID = false,
							ColumnIndex = 0,
							ContainsFilePath = false,
						},
						new FieldMapping
						{
							Field = "Custodian - Single Choice",
							ContainsID = false,
							ColumnIndex = 1,
							ContainsFilePath = false,
						},
						new FieldMapping
						{
							Field = "Email To",
							ContainsID = false,
							ColumnIndex = emailToColumnIndex,
							ContainsFilePath = false,
						},
						new FieldMapping
						{
							Field = "Extracted Text",
							ContainsID = false,
							ColumnIndex = extractedTextFilePathColumnIndex,
							ContainsFilePath = true,
						},
					},
				},
				Folder = new FolderSettings
				{
					FolderPathColumnIndex = null,
					RootFolderID = rootFolderId,
				},
				Other = new OtherSettings
				{
					ExtractedText = new ExtractedTextSettings
					{
						Encoding = null,
						ValidateEncoding = true,
					},
				},
			};

			// Example of data source configuration created without using DataSourceSettingsBuilder.
			DataSourceSettings dataSourceSettings = new DataSourceSettings
			{
				Type = DataSourceType.LoadFile,
				Path = "C:\\DefaultFileRepository\\samples\\load_file_04.dat",
				NewLineDelimiter = '#',
				ColumnDelimiter = '|',
				QuoteDelimiter = '^',
				MultiValueDelimiter = '$',
				NestedValueDelimiter = '&',
				Encoding = null,
				CultureInfo = "en-us",
				EndOfLine = DataSourceEndOfLine.Windows,
				FirstLineContainsColumnNames = true,
				StartLine = 0,
			};

			using (Relativity.Import.V1.Services.IDocumentConfigurationController documentConfiguration =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IDocumentConfigurationController>())

			using (Relativity.Import.V1.Services.IImportJobController importJobController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())

			using (Relativity.Import.V1.Services.IImportSourceController importSourceController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportSourceController>())
			{
				Response response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: "Sample-job-0007-doc-settings");

				if (this.IsPreviousResponseWithSuccess(response))
				{
					response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId, dataSourceSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
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
					var importProgress = await importJobController.GetProgressAsync(workspaceId, importId);

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
}

// Expected console output for provided sample load file.
// Data source state: Completed
// Import progress: Total records: 2, Imported records: 2, Records with errors: 0