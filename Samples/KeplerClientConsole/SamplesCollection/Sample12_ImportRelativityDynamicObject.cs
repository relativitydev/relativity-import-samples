// <copyright file="Sample12_ImportRelativityDynamicObject.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
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
		/// Domain object used in this example.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample12_ImportRelativityDynamicObject()
		{
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1035012;

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

					// .WithField(domainEmailBccColumnIndex, "Domains (Email BCC)")
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

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add import rdo settings to existing import job.
					response = await rdoConfiguration.CreateAsync(workspaceId, importId, importSettings);
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
				var dataSourceState = await this.WaitImportDataSourceToBeCompleted(
					() => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId),
					15000);

				// Get current import progress for specific data source.
				var importProgress = await importSourceController.GetProgressAsync(workspaceId, importId, sourceId);

				if (importProgress.IsSuccess)
				{
					Console.WriteLine($"\nData source state: {dataSourceState}");
					Console.WriteLine($"Import progress: Total records: {importProgress.Value.TotalRecords}, Imported records: {importProgress.Value.ImportedRecords}, Records with errors: {importProgress.Value.ErroredRecords}");
				}

				await importJobController.EndAsync(workspaceId, importId);
			}
		}
	}
}

// Expected output for sample load file.
// Data source state: Completed
// Import progress: Total records: 3, Imported records: 3, Records with errors: 0