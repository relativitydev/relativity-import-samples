// <copyright file="Sample22_ReadResponse.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
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
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1031725;

			ImportDocumentSettings invalidConfiguration = new ImportDocumentSettings();
			DataSourceSettings dataSourceSettings = new DataSourceSettings();

			using (Relativity.Import.V1.Services.IDocumentConfigurationController documentConfiguration =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IDocumentConfigurationController>())

			using (Relativity.Import.V1.Services.IImportJobController importJobController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())

			using (Relativity.Import.V1.Services.IImportSourceController importSourceController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportSourceController>())
			{
				Response response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: "Sample-job-00021");

				Console.WriteLine(response.IsSuccess);
				Console.WriteLine(response.ImportJobID);
				if (response.IsSuccess)
				{
					// try to configure job - use invalid configuration.
					Response configureResponse = await documentConfiguration.CreateAsync(workspaceId, importId, invalidConfiguration);
					if (!configureResponse.IsSuccess)
					{
						Console.WriteLine(configureResponse.ErrorCode);
						Console.WriteLine(configureResponse.ErrorMessage);
						Console.WriteLine(configureResponse.ImportJobID);
					}

					// try to add source to non existing job - use for example invalid configuration or invalid workspaceID .
					Response sourceResponse = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId, dataSourceSettings);
					if (!sourceResponse.IsSuccess)
					{
						Console.WriteLine(sourceResponse.ErrorCode);
						Console.WriteLine(sourceResponse.ErrorMessage);
						Console.WriteLine(sourceResponse.ImportJobID);
					}
				}
				else
				{
					Console.WriteLine(response.ErrorCode);
					Console.WriteLine(response.ErrorMessage);
					Console.WriteLine(response.ImportJobID);
				}
			}
		}
	}
}

// Example of output
/* 748d00c3-b755-40fd-81da-ff587ac055d2
C.CR.VLD.2001
Cannot create Job Configuration. Invalid import job configuration: Nothing is imported.
748d00c3-b755-40fd-81da-ff587ac055d2
S.CR.VLD.3001
Cannot create Data Source. Invalid load file settings: Delimiters are non-unique.
748d00c3-b755-40fd-81da-ff587ac055d2*/