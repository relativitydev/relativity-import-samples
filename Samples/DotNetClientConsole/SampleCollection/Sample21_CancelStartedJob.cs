// <copyright file="Sample21_CancelStartedJob.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetClient.SampleCollection
{
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
	using System.Net.Http.Json;
	using Relativity.Import.V1.Models;
	using Relativity.Import.Samples.DotNetClient.Helpers;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Builders.DataSource;
	using System.Text.Json.Serialization;
	using System.Text.Json;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading data source Ids.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample21_CancelStartedJob()
		{
			Console.WriteLine($"Running {nameof(Sample21_CancelStartedJob)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();

			// Destination workspace artifact Id.
			const int workspaceId = 1000000;

			const string loadFile07PathTemplate = "C:\\DefaultFileRepository\\samples\\load_file_07_{0}.dat";

			// set of columns indexes in load file used in import settings.
			const int controlNumberColumnIndex = 0;
			const int dataSourceCount = 20;
			const int filePathColumnIndex = 11;
			const int fileNameColumnIndex = 13;

			// Configuration settings for document import. Builder is used to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number"))
				.WithoutFolders();


			// Create payload for request.
			var importSettingPayload = new { importSettings };

			HttpClient httpClient = HttpClientHelper.CreateHttpClient();

			// Create payload for request.
			var createJobPayload = new
			{
				applicationName = "Import-service-sample-app",
				correlationID = $"Sample-job-0021"
			};

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


			// Add n data sources to the existing job.
			for (var i = 0; i < dataSourceCount; i++)
			{
				var dataSourceId = Guid.NewGuid();
				var path = string.Format(loadFile07PathTemplate, i);
				var dataSourceSettings = DataSourceSettingsBuilder.Create()
					.ForLoadFile(path)
					.WithDefaultDelimiters()
					.WithFirstLineContainingHeaders()
					.WithEndOfLineForWindows()
					.WithStartFromBeginning()
					.WithDefaultEncoding()
					.WithDefaultCultureInfo();

				// Create payload for request.
				var dataSourceSettingsPayload = new { dataSourceSettings };

				// Add data source settings to existing import job.
				// endpoint: POST /import-jobs/{importId}/sources/{sourceId}
				var importSourceUri = RelativityImportEndpoints.GetImportSourceUri(workspaceId, importId, dataSourceId);
				response = await httpClient.PostAsJsonAsync(importSourceUri, dataSourceSettingsPayload);
				await ImportJobSampleHelper.EnsureSuccessResponse(response);
			}

			// Start import job.
			// endpoint: POST /import-jobs/{importId}/begin
			var beginImportJobUri = RelativityImportEndpoints.GetImportJobBeginUri(workspaceId, importId);
			response = await httpClient.PostAsync(beginImportJobUri, null);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);
				
			// Read import job details
			await PrintImportJobStatus();

			// Cancel Job
			// endpoint: POST /import-jobs/{importId}/cancel
			var cancelJobUri = RelativityImportEndpoints.GetImportJobCancelUri(workspaceId, importId);
			response = await httpClient.PostAsync(cancelJobUri, null);
			await ImportJobSampleHelper.EnsureSuccessResponse(response);


			await PrintImportJobStatus();

			// Read import job details
			async Task PrintImportJobStatus()
			{
				Console.WriteLine("Import Job Status");
				
				// endpoint: GET import-jobs/{importId}/details"
				var importJobDetailsUri = RelativityImportEndpoints.GetImportJobDetailsUri(workspaceId, importId);

				JsonSerializerOptions options = new()
				{
					Converters = { new JsonStringEnumConverter() }
				};
				var importJobDetails = await httpClient.GetFromJsonAsync<ValueResponse<ImportDetails>>(importJobDetailsUri, options);
				if (importJobDetails != null)
				{
					Console.WriteLine($"	IsSuccess: {importJobDetails.IsSuccess}");
					Console.WriteLine($"	Import status: {importJobDetails.Value.State}");
				}
			}
		}
	}
}

/* Example of expected console result:
GetDetails:
        IsSuccess: True
        Import status: Scheduled
POST /Relativity.REST/api/import-service/v1/workspaces/1019056/import-jobs/f9317da7-2730-4648-b1a9-94c0098c712b/cancel
Response.IsSuccess: True
GetDetails:
        IsSuccess: True
        Import status: Canceled
*/