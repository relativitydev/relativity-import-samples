// <copyright file="Sample25_GetLocation_Job.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetClient.SampleCollection;

using System.Text;
using Relativity.Import.Samples.DotNetClient.Helpers;

/// <summary>
/// Class containing examples of using import service SDK.
/// </summary>
public partial class ImportServiceSample
{
	/// <summary>
	/// Example of getting default location for import job.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
	public async Task Sample25_GetLocation_Job()
	{
		// Targetted workspace ID
		int workspaceId = 1036722;

		// Targetted job GUID
		Guid jobID = Guid.Parse("c107191e-242f-450d-bddd-b1e6d4d58e96");

		using HttpClient client = HttpClientHelper.CreateHttpClient();
		var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

		var requestUri = RelativityImportEndpoints.GetImportSourceDefaultLocationUri(workspaceId, jobID);
		var response = await client.GetAsync(requestUri);
		var result = await ImportJobSampleHelper.EnsureSuccessValueResponse<string>(response);

		Console.WriteLine("Default location for job with ID {0} is: {1}", jobID, result.Value);
	}
}
