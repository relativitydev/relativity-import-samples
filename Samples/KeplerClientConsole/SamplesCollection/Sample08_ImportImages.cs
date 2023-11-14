// <copyright file="Sample08_ImportImages.cs" company="Relativity ODA LLC">
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
		/// Example of importing image files in append mode.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample08_ImportImages()
		{
			Console.WriteLine($"Running {nameof(Sample08_ImportImages)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			// Path to the file in opticon format used in data source settings.
			const string opticonFilePath = "\\files\\<TenantNumber>\\Files\\SampleDataSources\\opticon_01.opt";

			// Configuration settings for document import. Builder is used to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithoutNatives()
				.WithImages(i => i
					.WithAutoNumberImages()
					.WithoutProduction()
					.WithoutExtractedText()
					.WithFileTypeAutoDetection())
				.WithoutFieldsMapped()
				.WithoutFolders();

			// Configuration settings for data source. Builder is used to create settings.
			// ForOpticonFile(path) is used when importing images.
			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForOpticonFile(opticonFilePath)
				.WithDefaultDelimitersForOpticonFile()
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
					correlationID: "Sample-job-image-0008");

				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CreateAsync");

				// Add import images configuration to existing import job.
				response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				ResponseHelper.EnsureSuccessResponse(response, "IDocumentConfigurationController.CreateAsync");

				// Add data source settings to existing import job.
				response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId, dataSourceSettings);
				ResponseHelper.EnsureSuccessResponse(response, "IImportSourceController.AddSourceAsync");

				// Start import job.
				response = await importJobController.BeginAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.BeginAsync");

				// End import job.
				await importJobController.EndAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.EndAsync");

				// It may take some time for import job to be completed. Request data source details to monitor the current state.
				// NOTE: You can also request job details to verify if job is finished - see appropriate sample (sample_19).
				var dataSourceState = await this.WaitImportDataSourceToBeCompleted(
					funcAsync: () => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId),
					timeout: 10000);

				// Get current import progress.
				var importProgress = await importJobController.GetProgressAsync(workspaceId, importId);

				if (importProgress.IsSuccess)
				{
					Console.WriteLine($"\nData source state: {dataSourceState}");
					Console.WriteLine($"Import progress: Total records: {importProgress.Value.TotalRecords}, Imported records: {importProgress.Value.ImportedRecords}, Records with errors: {importProgress.Value.ErroredRecords}");
				}
			}
		}
	}
}

//// Expected output for sample load file.
// Data source state: Completed
// Import progress: Total records: 5, Imported records: 5, Records with errors: 0