// <copyright file="Sample21_CancelStartedJob.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler.SamplesCollection
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
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
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1031725;

			const int controlNumberColumnIndex = 0;
			const string loadFile01PathTemplate = "C:\\DefaultFileRepository\\samples\\load_file_07{0}.dat";

			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendOverlayMode(x => x
					.WithDefaultKeyField()
					.WithDefaultMultiFieldOverlayBehaviour())
				.WithoutNatives()
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

				if (this.IsPreviousResponseWithSuccess(response))
				{
					response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add n data sources to the existing job.
					for (int i = 0; i < 20; i++)
					{
						Guid dataSourceId = Guid.NewGuid();
						var path = string.Format(loadFile01PathTemplate, i);
						DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
							.ForLoadFile(path)
							.WithDefaultDelimiters()
							.WithFirstLineContainingHeaders()
							.WithEndOfLineForWindows()
							.WithStartFromBeginning()
							.WithDefaultEncoding()
							.WithDefaultCultureInfo();

						await importSourceController.AddSourceAsync(workspaceId, importId, dataSourceId, dataSourceSettings);
					}
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					response = await importJobController.BeginAsync(workspaceId, importId);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					var importDetails = await importJobController.GetDetailsAsync(workspaceId, importId);

					Console.WriteLine(importDetails.Value.State);

					await importJobController.CancelAsync(workspaceId, importId);

					importDetails = await importJobController.GetDetailsAsync(workspaceId, importId);

					Console.WriteLine(importDetails.Value.State);
				}
			}
		}
	}
}
