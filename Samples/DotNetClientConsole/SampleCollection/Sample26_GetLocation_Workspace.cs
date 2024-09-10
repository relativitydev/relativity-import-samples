// <copyright file="Sample26_GetLocation_Workspace.cs" company="Relativity ODA LLC">
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
	/// Example of getting default import location for workspace.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
	public async Task Sample26_GetLocation_Workspace()
	{
		// Targetted workspace ID
		int workspaceId = 1036722;

		using HttpClient client = HttpClientHelper.CreateHttpClient();
		var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

		var requestUri = RelativityImportEndpoints.GetImportJobDefaultLocationUri(workspaceId);
		var response = await client.GetAsync(requestUri);
		var result = await ImportJobSampleHelper.EnsureSuccessValueResponse<string>(response);

		Console.WriteLine("Default import location for workspace with ID {0} is: {1}", workspaceId, result.Value);
	}
}
