// <copyright file="Sample15_ReadImportRdoSettings.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.Samples.NetFrameworkClient.ImportSampleHelpers;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.Rdos;
	using Relativity.Import.V1.Models.Settings;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading ImportRdoSettings.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample15_ReadImportRdoSettings()
		{
			Console.WriteLine($"Running {nameof(Sample15_ReadImportRdoSettings)}");

			// GUID identifiers for import job.
			Guid importId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			const int nameColumnIndex = 2;
			const int valueColumnIndex = 3;
			const int parentObjectIdColumnIndex = 4;
			const int artifactTypeID = 100222;

			// Configuration of rdos import. Builder is used to create the settings.
			ImportRdoSettings importSettings = ImportRdoSettingsBuilder.Create()
				.WithAppendMode()
				.WithFieldsMapped(f => f
					.WithObjectFieldContainsID(nameColumnIndex, "RdoName")
					.WithField(valueColumnIndex, "RdoValue"))
				.WithRdo(f => f
					.WithArtifactTypeId(artifactTypeID)
					.WithParentColumnIndex(parentObjectIdColumnIndex));

			using (Relativity.Import.V1.Services.IRDOConfigurationController rdoConfiguration =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IRDOConfigurationController>())

			using (Relativity.Import.V1.Services.IImportJobController importJobController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())
			{
				// Create import job.
				Response response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: "Sample-job-import-0015");
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CreateAsync");

				// Add import rdo settings to existing import job.
				response = await rdoConfiguration.CreateAsync(workspaceId, importId, importSettings);
				ResponseHelper.EnsureSuccessResponse(response, "IDocumentConfigurationController.CreateAsync");

				// Read import rdo settings for existing import job.
				ValueResponse<ImportRdoSettings> settings = await rdoConfiguration.ReadAsync(workspaceId, importId);

				Console.WriteLine($"Read RDO settings: ArtifactTypeID:{settings?.Value.Rdo.ArtifactTypeID}, ParentColumnIndex:{settings?.Value.Rdo.ParentColumnIndex}");
			}
		}
	}

/* Expected console result:
	Read RDO settings: ArtifactTypeID:100222, ParentColumnIndex:2
 */
}
