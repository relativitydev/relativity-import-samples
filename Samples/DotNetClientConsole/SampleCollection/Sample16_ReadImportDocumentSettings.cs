// <copyright file="Sample16_ReadImportDocumentSettings.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetClient.SampleCollection
{
	using System;
	using System.Net.Http;
	using System.Net.Http.Json;
	using System.Text.Json;
	using System.Threading.Tasks;
	using Relativity.Import.Samples.DotNetClient.Helpers;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models.Settings;

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
			Console.WriteLine($"Running {nameof(this.Sample16_ReadImportDocumentSettings)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			// set of columns indexes in load file used in import settings.Example import of Domain RDO.
			const int controlNumberColumnIndex = 0;
			const int emailToColumnIndex = 11;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Create request's payload
			var createJobPayload = new
			{
				applicationName = "Import-service-sample-app",
				correlationID = "Sample-job-0016",
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
			var createImportJobUri = RelativityImportEndpoints.GetImportJobCreateUri(workspaceId, importId);

			var response = await httpClient.PostAsJsonAsync(createImportJobUri, createJobPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Add import document settings to existing import job (configure import job).
			// endpoint: POST /import-jobs/{importId}/documents-configurations
			var documentConfigurationUri = RelativityImportEndpoints.GetDocumentConfigurationUri(workspaceId, importId);
			response = await httpClient.PostAsJsonAsync(documentConfigurationUri, importSettingPayload);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);

			// Get import document settings for existing import job.
			// endpoint: GET /import-jobs/{importId}/documents-configurations
			var getResponse = await httpClient.GetAsync(documentConfigurationUri);

			// Check and print values
			var valueResponse = await ImportJobSampleHelper.EnsureSuccessValueResponse<ImportDocumentSettings>(getResponse);
			Console.WriteLine($"FieldMappings count: {valueResponse?.Value.Fields.FieldMappings.Length}");

			var json = JsonSerializer.Serialize(valueResponse, new JsonSerializerOptions()
			{
				WriteIndented = true,
			});

			Console.WriteLine(json);
		}
	}
}

/* Expected console result:
Response.IsSuccess: True
FieldMappings count: 2

{
  "Value": {
	"Overlay": null,
	"Native": {
	  "FilePathColumnIndex": 22,
	  "FileNameColumnIndex": 13
	},
	"Image": null,
	"Fields": {
	  "FieldMappings": [
		{
		  "ColumnIndex": 0,
		  "Field": "Control Number",
		  "ContainsID": false,
		  "ContainsFilePath": false
		},
		{
		  "ColumnIndex": 11,
		  "Field": "Email To",
		  "ContainsID": false,
		  "ContainsFilePath": false
		}
	  ]
	},
	"Folder": null,
	"Other": null
  },
  "IsSuccess": true,
  "ErrorMessage": null,
  "ErrorCode": null,
  "ImportJobID": "7ac85319-831c-4bfc-a8de-9d645c55fbd0"
  */