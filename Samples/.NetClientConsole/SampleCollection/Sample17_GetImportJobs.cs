// <copyright file="Sample17_GetImportJobs.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.Net7Client.SampleCollection
{
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models.Settings;
	using System.Net.Http.Json;
	using Relativity.Import.V1.Models;
	using Relativity.Import.Samples.Net7Client.Helpers;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading all import jobs Id.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample17_GetImportJobs()
		{
			// Destination workspace artifact Id.
			const int workspaceId = 1031725;

			const int importCount = 10;
			const int pageSize = 7;
			const int filePathColumnIndex = 11;
			const int fileNameColumnIndex = 13;

			// Configuration settings for document import. Builder is used to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithoutFieldsMapped()
				.WithoutFolders();


			HttpClient httpClient = HttpClientHelper.CreateHttpClient();

			// Create n import jobs.
			for (int i = 0; i < importCount; i++)
			{
				Guid importId = Guid.NewGuid();
				var createImportJobUri = RelativityImportEndpoints.GetCreateImportUri(workspaceId, importId);
				// Create payload for request.
				var createJobPayload = new
				{
					applicationName = "Import-service-sample-app",
					correlationID = $"Sample-job-0017-GetImportJobs_{i}"
				};

				// Create import job.
				// endpoint: POST /import-jobs/{importId}
				var response = await httpClient.PostAsJsonAsync(createImportJobUri, createJobPayload);
				await ImportJobSampleHelper.EnsureSuccessResponse(response);

				// Start import job.
				// endpoint: POST /import-jobs/{importId}/begin
				var beginImportJobUri = RelativityImportEndpoints.GetBeginJobUri(workspaceId, importId);
				response = await httpClient.PostAsync(beginImportJobUri, null);
				await ImportJobSampleHelper.EnsureSuccessResponse(response);

			}

			// Read import job collection (guid list) for particular workspace. Paginating is supported thanks to dedicated parameters.
			var getJobsUri = RelativityImportEndpoints.GetImportUri(workspaceId, 0, pageSize);

			ValueResponse<ImportJobs>? valueResponse =
				await httpClient.GetFromJsonAsync<ValueResponse<ImportJobs>>(getJobsUri);


			if (valueResponse is {IsSuccess: true})
			{
				Console.WriteLine($"Total jobs amount: {valueResponse.Value.TotalCount}");

				foreach (var importJobId in valueResponse.Value.Jobs)
				{
					Console.WriteLine($"Job Id: {importJobId}");

					// End jobs
					var endImportJobUri = RelativityImportEndpoints.GetEndJobUri(workspaceId, importJobId);
					var response = await httpClient.PostAsync(endImportJobUri, null);
					await ImportJobSampleHelper.EnsureSuccessResponse(response);
				}

			}
		}
	}
}
