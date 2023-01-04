// <copyright file="Sample22_ReadResponse.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;

	using Relativity.Import.Samples.NetFrameworkClient.ImportSampleHelpers;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of canceling started job.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample22_ReadResponse()
		{
			Console.WriteLine($"Running {nameof(Sample22_ReadResponse)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			ImportDocumentSettings invalidConfiguration = new ImportDocumentSettings();
			DataSourceSettings dataSourceSettings = new DataSourceSettings();

			using (Relativity.Import.V1.Services.IDocumentConfigurationController documentConfiguration =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IDocumentConfigurationController>())

			using (Relativity.Import.V1.Services.IImportJobController importJobController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())

			using (Relativity.Import.V1.Services.IImportSourceController importSourceController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportSourceController>())
			{
				// Create import job.
				Response response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: "Sample-job-00022");

				Console.WriteLine($"IsSuccess:{response?.IsSuccess}");
				Console.WriteLine($"ImportJobID:{response?.ImportJobID}");
				Console.WriteLine($"ErrorCode:{response?.ErrorCode}");
				Console.WriteLine($"ErrorMessage:{response?.ErrorMessage}");

				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CreateAsync");

				// try to configure job - use invalid configuration.
				Response configureResponse = await documentConfiguration.CreateAsync(workspaceId, importId, invalidConfiguration);

				Console.WriteLine("Add document configuration response");
				Console.WriteLine($"IsSuccess:{response?.IsSuccess}");
				if (!configureResponse.IsSuccess)
				{
					Console.WriteLine($"ImportJobID:{configureResponse?.ImportJobID}");
					Console.WriteLine($"ErrorCode:{configureResponse?.ErrorCode}");
					Console.WriteLine($"ErrorMessage:{configureResponse?.ErrorMessage}");
				}


				// try to add source to non existing job - use for example invalid configuration or invalid workspaceID .
				Response sourceResponse = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId, dataSourceSettings);
				Console.WriteLine("Add Data source response:");
				Console.WriteLine($"IsSuccess:{response?.IsSuccess}");
				if (!sourceResponse.IsSuccess)
				{
					Console.WriteLine($"ImportJobID:{sourceResponse?.ImportJobID}");
					Console.WriteLine($"ErrorCode:{sourceResponse?.ErrorCode}");
					Console.WriteLine($"ErrorMessage:{sourceResponse?.ErrorMessage}");
				}

				// try to start job
				response = await importJobController.BeginAsync(workspaceId, importId);
				Console.WriteLine("Begin Job response:");
				if (!response.IsSuccess)
				{
					Console.WriteLine($"ImportJobID:{response?.ImportJobID}");
					Console.WriteLine($"ErrorCode:{response?.ErrorCode}");
					Console.WriteLine($"ErrorMessage:{response?.ErrorMessage}");
				}

			}
		}
	}
}

// Example of output
/*
 IImportJobController.CreateAsync
IsSuccess:True
ImportJobID:bf272e8c-c26b-435f-a21b-14a3f46661e2
ErrorCode:
ErrorMessage:

Add document configuration response
IsSuccess:True
ImportJobID:d612bf6a-b044-4c6b-b417-07b135c163fe
ErrorCode:C.CR.VLD.2001
ErrorMessage:Cannot create Job Configuration. Invalid import job configuration: Nothing is imported.

Add Data source response:
IsSuccess:True
ImportJobID:d612bf6a-b044-4c6b-b417-07b135c163fe
ErrorCode:S.CR.VLD.3001
ErrorMessage:Cannot create Data Source. Invalid load file settings: Delimiters are non-unique.

Begin Job response:
ImportJobID:d612bf6a-b044-4c6b-b417-07b135c163fe
ErrorCode:J.BEG.VLD.1508
ErrorMessage:Cannot begin Import Job. Job is not configured. Current Job state: New
*/