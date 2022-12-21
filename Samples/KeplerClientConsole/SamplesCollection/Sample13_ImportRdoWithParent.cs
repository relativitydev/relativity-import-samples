// <copyright file="Sample13_ImportRdoWithParent.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.DataSource;
	using Relativity.Import.V1.Builders.Rdos;
	using Relativity.Import.V1.Models;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of import relativity dynamic objects (rdo) with selecting its parent.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample13_ImportRdoWithParent()
		{
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.

			// set of columns indexes in load file used in import settings.
			const int workspaceId = 5;

			// set of columns indexes in load file used in import settings.
			const int nameIdColumnIndex = 2;
			const int valueColumnIndex = 3;
			const int parentObjectIdColumnIndex = 4;

			// RDO artifact type id
			const int rdoArtifactTypeID = 1000027;

			// Path to the load file used in data source settings.
			const string loadFilePath = "/tenantName/sampleWorkspaceID/sampleImportId/file001.dat";

			// Configuration RDO settings for Relativity Dynamic Objects (RDOs) import. Builder is used to create settings.
			ImportRdoSettings importSettings = ImportRdoSettingsBuilder.Create()
				.WithAppendMode()
				.WithFieldsMapped(f => f
					.WithObjectFieldContainsID(nameIdColumnIndex, "nameID")
					.WithField(valueColumnIndex, "Value"))
				.WithRdo(f => f
					.WithArtifactTypeId(rdoArtifactTypeID)
					.WithParentColumnIndex(parentObjectIdColumnIndex));

			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForLoadFile(loadFilePath)
				.WithDefaultDelimiters()
				.WithoutFirstLineContainingHeaders()
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
				Response response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: "Sample-job-import-00013");

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

				if (this.IsPreviousResponseWithSuccess(response))
				{
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
}
