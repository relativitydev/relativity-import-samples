// <copyright file="Sample24_DeleteSource.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetClient.SampleCollection;

using System.Text;
using Relativity.Import.Samples.DotNetClient.Helpers;

/// <summary>
///  Class containing examples of using import service SDK.
/// </summary>
public partial class ImportServiceSample
{
	/// <summary>
	/// Example of deleting a folder associated with a specific workspace, job, and source in Relativity.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
	public async Task Sample24_DeleteSource()
	{
		// Targetted workspace ID
		int workspaceId = 1000000;

		// Targetted job GUID
		Guid jobID = Guid.Parse("00000000-0000-0000-0000-000000000000");

		// Targetted job source GUID
		Guid sourceID = Guid.Parse("00000000-0000-0000-0000-000000000000");

		using HttpClient client = HttpClientHelper.CreateHttpClient();
		var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

		var requestUri = RelativityImportEndpoints.GetImportSourceDeleteUri(workspaceId, jobID, sourceID);
		var response = await client.PostAsync(requestUri, content);
		await ImportJobSampleHelper.EnsureSuccessResponse(response);
	}
}
