// <copyright file="Sample16_ReadImportDocumentSettings.cs" company="Relativity ODA LLC">
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
	using Relativity.Import.V1.Builders.Documents;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading Import Document Settings for existing job.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample16_ReadImportDocumentSettings()
		{
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1019056;

			// set of columns indexes in load file used in import settings.Example import of Domain RDO.
			const int controlNumberColumnIndex = 0;
			const int emailToColumnIndex = 11;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Create request's payload
			var createJobPayload = new
			{
				applicationName = "Import-service-sample-app",
				correlationID = "Sample-job-0015"
			};

			// Configuration settings for document import. Builder is used to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number")
					.WithField(emailToColumnIndex, "Email To"))
				.WithoutFolders();


			// Create payload for request.
			var importSettingPayload = new { importSettings };

			HttpClient httpClient = HttpClientHelper.CreateHttpClient();

			// Create import job.
			// endpoint: POST /import-jobs/{importId}
			var createImportJobUri = RelativityImportEndpoints.GetCreateImportUri(workspaceId, importId);

			var response = await httpClient.PostAsJsonAsync(createImportJobUri, createJobPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Add import document settings to existing import job (configure import job).
			// endpoint: POST /import-jobs/{importId}/documents-configurations
			var documentConfigurationUri = RelativityImportEndpoints.GetDocumentConfigurationUri(workspaceId, importId);
			response = await httpClient.PostAsJsonAsync(documentConfigurationUri, importSettingPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			var getResponse = await httpClient.GetFromJsonAsync<dynamic>(documentConfigurationUri);
			await ImportJobSampleHelper.EnsureSuccessResponse(getResponse);

			Console.WriteLine(getResponse?.Value.Rdo.ArtifactTypeID);
		}
	}
}
