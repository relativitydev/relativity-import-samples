// <copyright file="Sample12_ImportRelativityDynamicObject.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.Samples.DotNetFrameworkClient.ImportSampleHelpers;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.DataSource;
	using Relativity.Import.V1.Builders.Rdos;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of import  Relativity Dynamic Object (RDO).
		/// NOTE: Existing RDO "Domain" is used in this example. Please insert documents from sample01 first.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample12_ImportRelativityDynamicObject()
		{
			Console.WriteLine($"Running {nameof(Sample12_ImportRelativityDynamicObject)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			// set of columns indexes in load file used in import settings.
			// example import of Domain RDO.
			const int nameColumnIndex = 0;
			const int domainEmailCcColumnIndex = 3;
			const int domainEmailFromColumnIndex = 4;
			const int domainEmailToColumnIndex = 5;

			// RDO artifact type id
			const int domainArtifactTypeID = 1000027;

			// Path to the load file used in data source settings.
			const string rdoLoadFile = "C:\\DefaultFileRepository\\samples\\rdo_load_file_01.dat";

			// Configuration RDO settings for Relativity Dynamic Objects (RDOs) import. Builder is used to create settings.
			ImportRdoSettings importSettings = ImportRdoSettingsBuilder.Create()
				.WithAppendMode()
				.WithFieldsMapped(f => f
					.WithField(nameColumnIndex, "Name")
					// If you do not use these fields please just comment them. Otherwise use sample01 first to import related documents.
					.WithField(domainEmailCcColumnIndex, "Domains (Email CC)")
					.WithField(domainEmailFromColumnIndex, "Domains (Email From)")
					.WithField(domainEmailToColumnIndex, "Domains (Email To)"))
				.WithRdo(r => r
					.WithArtifactTypeId(domainArtifactTypeID)
					.WithoutParentColumnIndex());

			// Configuration settings for data source. Builder is used to create settings.
			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForLoadFile(rdoLoadFile)
				.WithDefaultDelimiters()
				.WithFirstLineContainingHeaders()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();

			using (Relativity.Import.V1.Services.IRDOConfigurationController rdoConfiguration =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IRDOConfigurationController>())

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
					correlationID: "Sample-job-import-0012");

				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CreateAsync");

				// Add import rdo settings to existing import job.
				response = await rdoConfiguration.CreateAsync(workspaceId, importId, importSettings);
				ResponseHelper.EnsureSuccessResponse(response, "IRDOConfigurationController.CreateAsync");

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
				var dataSourceState = await this.WaitImportDataSourceToBeCompleted(
					() => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId),
					10000);

				// Get current import progress for specific data source.
				var importProgress = await importSourceController.GetProgressAsync(workspaceId, importId, sourceId);

				if (importProgress.IsSuccess)
				{
					Console.WriteLine($"\nData source state: {dataSourceState}");
					Console.WriteLine($"Import progress: Total records: {importProgress.Value.TotalRecords}, Imported records: {importProgress.Value.ImportedRecords}, Records with errors: {importProgress.Value.ErroredRecords}");
				}

			}
		}
	}
}

// Expected output for sample load file.
// Data source state: Completed
// Import progress: Total records: 3, Imported records: 3, Records with errors: 0