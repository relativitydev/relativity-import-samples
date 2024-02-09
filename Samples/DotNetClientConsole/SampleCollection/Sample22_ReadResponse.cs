// <copyright file="Sample22_ReadResponse.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetClient.SampleCollection
{
	using System;
	using System.Net.Http.Json;
	using System.Threading.Tasks;
	using Relativity.Import.Samples.DotNetClient.Helpers;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of simple import native files.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample22_ReadResponse()
		{
			Console.WriteLine($"Running {nameof(this.Sample22_ReadResponse)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			// set of columns indexes in load file used in import settings.
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Create payload for request.
			var createJobPayload = new
			{
				applicationName = "Import-service-sample-app",
				correlationID = "Sample-job-0022",
			};

			// Configuration settings for document import. Builder is used to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithoutFieldsMapped()
				.WithoutFolders();

			// Create payload for request.
			var importSettingPayload = new { importSettings };

			// Configuration settings for data source. Builder is used to create settings.
			DataSourceSettings dataSourceSettings = new ();

			// Create payload for request.
			var dataSourceSettingsPayload = new { dataSourceSettings };

			var httpClient = HttpClientHelper.CreateHttpClient();

			// Create import job.
			// endpoint: POST /import-jobs/{importId}
			var createImportJobUri = RelativityImportEndpoints.GetImportJobCreateUri(workspaceId, importId);

			var responseMessage = await httpClient.PostAsJsonAsync(createImportJobUri, createJobPayload);
			var response = await HttpClientHelper.DeserializeResponse<Response>(responseMessage);

			ImportJobSampleHelper.ConsoleWriteLine("Create import job response:", ConsoleColor.DarkGreen);

			Console.WriteLine($"IsSuccess:{response?.IsSuccess}");
			Console.WriteLine($"ImportJobID:{response?.ImportJobID}");
			Console.WriteLine($"ErrorCode:{response?.ErrorCode}");
			Console.WriteLine($"ErrorMessage:{response?.ErrorMessage}");

			// Add import document settings to existing import job (configure import job).
			// endpoint: POST /import-jobs/{importId}/documents-configurations
			var documentConfigurationUri = RelativityImportEndpoints.GetDocumentConfigurationUri(workspaceId, importId);
			responseMessage = await httpClient.PostAsJsonAsync(documentConfigurationUri, importSettingPayload);
			response = await HttpClientHelper.DeserializeResponse<Response>(responseMessage);

			ImportJobSampleHelper.ConsoleWriteLine("Add document configuration response", ConsoleColor.DarkGreen);

			Console.WriteLine($"IsSuccess:{response?.IsSuccess}");
			Console.WriteLine($"ImportJobID:{response?.ImportJobID}");
			Console.WriteLine($"ErrorCode:{response?.ErrorCode}");
			Console.WriteLine($"ErrorMessage:{response?.ErrorMessage}");

			// Add data source settings to existing import job.
			// endpoint: POST /import-jobs/{importId}/sources/{sourceId}
			var importSourcesUri = RelativityImportEndpoints.GetImportSourceUri(workspaceId, importId, sourceId);
			responseMessage = await httpClient.PostAsJsonAsync(importSourcesUri, dataSourceSettingsPayload);
			response = await HttpClientHelper.DeserializeResponse<Response>(responseMessage);

			ImportJobSampleHelper.ConsoleWriteLine("Add Data source response:", ConsoleColor.DarkGreen);

			Console.WriteLine($"IsSuccess:{response?.IsSuccess}");
			Console.WriteLine($"ImportJobID:{response?.ImportJobID}");
			Console.WriteLine($"ErrorCode:{response?.ErrorCode}");
			Console.WriteLine($"ErrorMessage:{response?.ErrorMessage}");

			// Start import job.
			// endpoint: POST /import-jobs/{importId}/begin
			var beginImportJobUri = RelativityImportEndpoints.GetImportJobBeginUri(workspaceId, importId);
			responseMessage = await httpClient.PostAsync(beginImportJobUri, null);
			response = await HttpClientHelper.DeserializeResponse<Response>(responseMessage);

			ImportJobSampleHelper.ConsoleWriteLine("Begin job response:", ConsoleColor.DarkGreen);

			Console.WriteLine($"IsSuccess:{response?.IsSuccess}");
			Console.WriteLine($"ImportJobID:{response?.ImportJobID}");
			Console.WriteLine($"ErrorCode:{response?.ErrorCode}");
			Console.WriteLine($"ErrorMessage:{response?.ErrorMessage}");
		}
	}
}

/* Expected console result:
	Create import job response:
	IsSuccess:True
	ImportJobID:bf272e8c-c26b-435f-a21b-14a3f46661e2
	ErrorCode:
	ErrorMessage:

	Add document configuration response
	IsSuccess:False
	ImportJobID:bf272e8c-c26b-435f-a21b-14a3f46661e2
	ErrorCode:C.CR.VLD.2001
	ErrorMessage:Cannot create Job Configuration. Invalid import job configuration: Nothing is imported; Fields property is not set when importing Natives.

	Add Data source response:
	IsSuccess:False
	ImportJobID:bf272e8c-c26b-435f-a21b-14a3f46661e2
	ErrorCode:S.CR.VLD.3001
	ErrorMessage:Cannot create Data Source. Invalid load file settings: Delimiters are non-unique.

	Begin job response:
	IsSuccess:False
	ImportJobID:bf272e8c-c26b-435f-a21b-14a3f46661e2
	ErrorCode:J.BEG.VLD.1508
	ErrorMessage:Cannot begin Import Job. Job is not configured. Current Job state: New
*/
