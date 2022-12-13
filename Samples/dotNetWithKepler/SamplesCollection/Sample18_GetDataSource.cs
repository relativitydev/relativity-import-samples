// <copyright file="Sample18_GetDataSource.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.DataSource;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading all data sources Ids for particular job.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample18_GetDataSource()
		{
			// destination workspace artifact Id.
			const int workspaceId = 1031725;

			const int importCount = 10;
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

			using (Relativity.Import.V1.Services.IDocumentConfigurationController documentConfiguration =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IDocumentConfigurationController>())
			using (Relativity.Import.V1.Services.IImportJobController importJobController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())
			using (Relativity.Import.V1.Services.IImportSourceController importSourceController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportSourceController>())
			{
				// Create import job.
				Guid importId = Guid.NewGuid();
				await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: $"Sample-job-0018");

				// Add import document settings to existing import job.
				await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);

				// Add n data sources to the existing job.
				for (int i = 0; i < importCount; i++)
				{
					Guid dataSourceId = Guid.NewGuid();

					DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
						.ForLoadFile($"host/Sample18/loadFile{i}")
						.WithDefaultDelimiters()
						.WithFirstLineContainingHeaders()
						.WithEndOfLineForWindows()
						.WithStartFromBeginning()
						.WithDefaultEncoding()
						.WithDefaultCultureInfo();

					await importSourceController.AddSourceAsync(workspaceId, importId, dataSourceId, dataSourceSettings);
				}

				// Read collection of data sources for particular import job.
				ValueResponse<DataSources> dataSources = await importJobController.GetSourcesAsync(workspaceId, importId);

				Console.WriteLine(dataSources.Value.TotalCount);
				foreach (var sources in dataSources.Value.Sources)
				{
					Console.WriteLine(sources.ToString());
				}
			}
		}
	}
}
