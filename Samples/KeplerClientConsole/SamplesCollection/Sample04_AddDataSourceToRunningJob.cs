// <copyright file="Sample04_AddDataSourceToRunningJob.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetFrameworkClient.SamplesCollection
{
	using System;
	using System.Collections.Generic;
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
		/// Example of adding another data source when job is already running.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample04_AddDataSourceToRunningJob()
		{
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid source01Id = Guid.NewGuid();
			Guid source02Id = Guid.NewGuid();
			List<Guid> addedSources = new List<Guid>();

			// destination workspace artifact Id.
			const int workspaceId = 1031725;

			// set of columns indexes in load file used in import settings.
			const int controlNumberColumnIndex = 0;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Path to the load files used in data source settings.
			const string loadFile01Path = "C:\\DefaultFileRepository\\samples\\load_file_01.dat";
			const string loadFile02Path = "C:\\DefaultFileRepository\\samples\\load_file_02.dat";

			// Configuration settings for document import. Use builder to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number"))
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
					correlationID: "Sample-job-0004");

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add import document settings to existing import job.
					response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add data source settings to existing import job - before job started.
					response = await importSourceController.AddSourceAsync(workspaceId, importId, source01Id, dataSourceSettings01);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					addedSources.Add(source01Id);
					response = await importJobController.BeginAsync(workspaceId, importId);
				}

				if (!this.IsPreviousResponseWithSuccess(response, nameof(IImportJobController.BeginAsync)))
				{
					Console.WriteLine($"Import Job was not started because of:  {response.ErrorCode} - {response.ErrorMessage}");
					return;
				}

				// Add second data source settings to existing and started import job.
				response = await importSourceController.AddSourceAsync(workspaceId, importId, source02Id, dataSourceSettings02);

				if (this.IsPreviousResponseWithSuccess(response, nameof(IImportSourceController.AddSourceAsync)))
				{
					addedSources.Add(source02Id);
				}

				// Read sourceId guid collection (added to job successfully) for particular job to request the progress each of them.
				var sources = await importJobController.GetSourcesAsync(workspaceId, importId);
				if (sources.IsSuccess)
				{
					foreach (var sourceId in sources.Value.Sources)
					{
						// It may take some time for import job to be completed. Request data source details to monitor the current state.
						var dataSourceState = await this.WaitToStatusChange(
							targetStatus: DataSourceState.Completed,
							funcAsync: () => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId),
							timeout: 10000);

						// Get current import progress for specific data source.
						var importProgress = await importSourceController.GetProgressAsync(workspaceId, importId, sourceId);

						if (importProgress.IsSuccess)
						{
							Console.WriteLine($"Data source [{sourceId}] state: {dataSourceState}");
							Console.WriteLine($"Import progress: Total records: {importProgress.Value.TotalRecords}, Imported records: {importProgress.Value.ImportedRecords}, Records with errors: {importProgress.Value.ErroredRecords}");
						}
					}
				}
			}
		}
	}
}
