// <copyright file="Sample21_CancelStartedJob.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.Samples.NetFrameworkClient.ImportSampleHelpers;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.DataSource;
	using Relativity.Import.V1.Builders.Documents;
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
		public async Task Sample21_CancelStartedJob()
		{
			Console.WriteLine($"Running {nameof(Sample21_CancelStartedJob)}");

			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			// set of columns indexes in load file used in import settings.
			const int controlNumberColumnIndex = 0;
			const int filePathColumnIndex = 11;
			const int fileNameColumnIndex = 13;

			const string loadFile07PathTemplate = "C:\\DefaultFileRepository\\samples\\load_file_07{0}.dat";

			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendOverlayMode(x => x
					.WithDefaultKeyField()
					.WithDefaultMultiFieldOverlayBehaviour())
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number"))
					.WithoutFolders();

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
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CreateAsync");

				response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				ResponseHelper.EnsureSuccessResponse(response, "IDocumentConfigurationController.CreateAsync");

				// Add n data sources to the existing job.
				for (int i = 0; i < 20; i++)
				{
					Guid dataSourceId = Guid.NewGuid();
					var path = string.Format(loadFile07PathTemplate, i);
					DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
						.ForLoadFile(path)
						.WithDefaultDelimiters()
						.WithFirstLineContainingHeaders()
						.WithEndOfLineForWindows()
						.WithStartFromBeginning()
						.WithDefaultEncoding()
						.WithDefaultCultureInfo();

					await importSourceController.AddSourceAsync(workspaceId, importId, dataSourceId, dataSourceSettings);
					ResponseHelper.EnsureSuccessResponse(response, "IImportSourceController.AddSourceAsync");
				}

				// Start import job.
				response = await importJobController.BeginAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.BeginAsync");

				// Read import job details.
				var valueResponse = await importJobController.GetDetailsAsync(workspaceId, importId);
				Console.WriteLine("Import Job Status");
				Console.WriteLine($"	IsSuccess: {valueResponse.IsSuccess}");
				Console.WriteLine($"	Import status: {valueResponse.Value.State}");

				await importJobController.CancelAsync(workspaceId, importId);
				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CancelAsync");

				// Read import job details once again.
				valueResponse = await importJobController.GetDetailsAsync(workspaceId, importId);
				Console.WriteLine("Import Job Status");
				Console.WriteLine($"	IsSuccess: {valueResponse.IsSuccess}");
				Console.WriteLine($"	Import status: {valueResponse.Value.State}");

			}
		}
	}
}
/* Example of expected console result:
IImportJobController.BeginAsync
Response.IsSuccess: True
Import Job Status
        IsSuccess: True
        Import status: Scheduled
IImportJobController.CancelAsync
Response.IsSuccess: True
Import Job Status
        IsSuccess: True
        Import status: Canceled
*/