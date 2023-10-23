// <copyright file="Sample15_ReadImportRdoSettings.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.Net7Client.SampleCollection
{
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Relativity.Import.V1.Models.Settings;
	using System.Net.Http.Json;
	using Relativity.Import.V1.Builders.Rdos;
	using Relativity.Import.Samples.Net7Client.Helpers;

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

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			// set of columns indexes in load file used in import settings.
			const int nameColumnIndex = 2;
			const int valueColumnIndex = 3;
			const int parentObjectIdColumnIndex = 4;
			const int artifactTypeID = 1000222;

			// Create request's payload
			var createJobPayload = new
			{
				applicationName = "Import-service-sample-app",
				correlationID = "Sample-job-0015"
			};

			// Configuration RDO settings for Relativity Dynamic Objects (RDOs) import. Builder is used to create settings.
			ImportRdoSettings importSettings = ImportRdoSettingsBuilder.Create()
				.WithAppendMode()
				.WithFieldsMapped(f => f
					.WithObjectFieldContainsID(nameColumnIndex, "RdoName")
					.WithField(valueColumnIndex, "RdoValue"))
				.WithRdo(f => f
					.WithArtifactTypeId(artifactTypeID)
					.WithParentColumnIndex(parentObjectIdColumnIndex));


			// Create payload for request.
			var importSettingPayload = new { importSettings };

			HttpClient httpClient = HttpClientHelper.CreateHttpClient();

			// Create import job.
			// endpoint: POST /import-jobs/{importId}
			var createImportJobUri = RelativityImportEndpoints.GetImportJobCreateUri(workspaceId, importId);

			var response = await httpClient.PostAsJsonAsync(createImportJobUri, createJobPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Add import document settings to existing import job (configure import job).
			// endpoint: POST /import-jobs/{importId}/rdos-configurations
			var rdoConfigurationUri = RelativityImportEndpoints.GetRdoConfigurationUri(workspaceId, importId);
			response = await httpClient.PostAsJsonAsync(rdoConfigurationUri, importSettingPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			var getResponse = await httpClient.GetAsync(rdoConfigurationUri);
			var valueResponse = await ImportJobSampleHelper.EnsureSuccessValueResponse<ImportRdoSettings>(getResponse);

			Console.WriteLine($"Read RDO settings: ArtifactTypeID:{valueResponse?.Value.Rdo.ArtifactTypeID}, ParentColumnIndex:{valueResponse?.Value.Rdo.ParentColumnIndex}");
		}
	}
}

/* Expected console result:
	Read RDO settings: ArtifactTypeID:100222, ParentColumnIndex:4
*/