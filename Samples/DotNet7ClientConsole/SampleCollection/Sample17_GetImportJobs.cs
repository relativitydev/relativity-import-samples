// <copyright file="Sample17_GetImportJobs.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.Net7Client.SampleCollection
{
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
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
			Console.WriteLine($"Running {nameof(Sample17_GetImportJobs)}");

			// Destination workspace artifact Id.
			const int workspaceId = 1000000;

			const int importCount = 7;

			// Read import job collection (guid list) for particular workspace.
			HttpClient httpClient = HttpClientHelper.CreateHttpClient();

			var getJobsUri = RelativityImportEndpoints.GetImportJobsForWorkspaceUri(workspaceId, 0, 1);

			var response = await httpClient.GetAsync(getJobsUri);
			var valueResponse = await ImportJobSampleHelper.EnsureSuccessValueResponse<ImportJobs>(response);
			if (valueResponse is {IsSuccess: true})
			{
				Console.WriteLine($"Import Jobs total count: {valueResponse.Value.TotalCount}");
			}

			const int length = 7;

			// Create n import jobs.
			for (var i = 0; i < importCount; i++)
			{
				Console.WriteLine($"Creating {importCount} jobs");
				Guid importId = Guid.NewGuid();
				var createImportJobUri = RelativityImportEndpoints.GetImportJobCreateUri(workspaceId, importId);
				// Create payload for request.
				var createJobPayload = new
				{
					applicationName = "Import-service-sample-app",
					correlationID = $"Sample-job-0017-GetImportJobs_{i}"
				};

				// Create import job.
				// endpoint: POST /import-jobs/{importId}
				response = await httpClient.PostAsJsonAsync(createImportJobUri, createJobPayload);
				await ImportJobSampleHelper.EnsureSuccessResponse(response);
			}

			// Read import job collection (guid list) for particular workspace. Paginating is supported thanks to dedicated parameters.
			getJobsUri = RelativityImportEndpoints.GetImportJobsForWorkspaceUri(workspaceId, 0, length);

			valueResponse =
				await httpClient.GetFromJsonAsync<ValueResponse<ImportJobs>>(getJobsUri);

			if (valueResponse is {IsSuccess: true})
			{
				Console.WriteLine($"Jobs total count: {valueResponse.Value.TotalCount}");
				Console.WriteLine($"ImportJobIds:");
				foreach (var importJobId in valueResponse.Value.Jobs)
				{
					Console.WriteLine(importJobId);
				}
			}
		}
	}
}

/* Example of console result 
	Response.IsSuccess: True
	Jobs total count: 15
	ImportJobIds:
	39753e22-a948-4c74-8ebd-3abd9fa47473
	8986cb61-8f1f-4ad7-96c0-8dc3f229fd1c
	33504c27-0bb2-46cd-9651-e539a4ae672f
	b83d8a04-320e-4256-93dc-4957e9908d14
	8b2ad1aa-c18e-4ccd-a1b3-9cacdf2d1ce6
	1bac695d-bab2-41fb-a6f7-0d995c5e6871
	a98b009e-01b1-4b3c-96b4-491d33fb5827
*/