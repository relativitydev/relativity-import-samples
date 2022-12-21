// <copyright file="Sample20_GetDataSourceProgress.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetFrameworkClient.SamplesCollection
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
		public async Task Sample20_GetDataSourceProgress()
		{
			Guid importId = Guid.NewGuid();
			Guid sourceId01 = Guid.NewGuid();
			Guid sourceId02 = Guid.NewGuid();
			const int workspaceId = 1031725;

			const int controlNumberColumnIndex = 0;
			const int custodianColumnIndex = 1;
			const int dateSentColumnIndex = 5;
			const int emailToColumnIndex = 11;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			const string loadFile01Path = "C:\\DefaultFileRepository\\samples\\load_file_01.dat";
			const string loadFile02Path = "C:\\DefaultFileRepository\\samples\\load_file_02.dat";

			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number")
					.WithField(custodianColumnIndex, "Custodian - Single Choice")
					.WithField(emailToColumnIndex, "Email To")
					.WithField(dateSentColumnIndex, "Date Sent"))
				.WithoutFolders();

			DataSourceSettings dataSourceSettings01 = DataSourceSettingsBuilder.Create()
				.ForLoadFile(loadFile01Path)
				.WithDelimiters(d => d
					.WithColumnDelimiters('|')
					.WithQuoteDelimiter('^')
					.WithNewLineDelimiter('#')
					.WithNestedValueDelimiter('&')
					.WithMultiValueDelimiter('$'))
				.WithFirstLineContainingHeaders()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();

			DataSourceSettings dataSourceSettings02 = DataSourceSettingsBuilder.Create()
				.ForLoadFile(loadFile02Path)
				.WithDelimiters(d => d
					.WithColumnDelimiters('|')
					.WithQuoteDelimiter('^')
					.WithNewLineDelimiter('#')
					.WithNestedValueDelimiter('&')
					.WithMultiValueDelimiter('$'))
				.WithoutFirstLineContainingHeaders()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();

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
					correlationID: "Sample-job-00020");

				if (this.IsPreviousResponseWithSuccess(response))
				{
					response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId01, dataSourceSettings01);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId02, dataSourceSettings02);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					response = await importJobController.BeginAsync(workspaceId, importId);
				}

				// It may take some time for import job to be completed. Request data source details to monitor the current state.
				var dataSourceState01 = await this.WaitToStatusChange(
					targetStatus: DataSourceState.Completed,
					funcAsync: () => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId01),
					timeout: 10000);

				var dataSourceState02 = await this.WaitToStatusChange(
					targetStatus: DataSourceState.Completed,
					funcAsync: () => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId02),
					timeout: 10000);

				ValueResponse<DataSourceDetails> dataSourceDetails = await importSourceController.GetDetailsAsync(workspaceId, importId, sourceId01);

				Console.WriteLine($"State: {dataSourceDetails.Value.State} DataSourceSettings: {dataSourceDetails.Value.DataSourceSettings.Path} etc.");

				// Get Data source Progress
				ValueResponse<ImportProgress> importProgress01 = await importSourceController.GetProgressAsync(workspaceId, importId, sourceId01);
				ValueResponse<ImportProgress> importProgress02 = await importSourceController.GetProgressAsync(workspaceId, importId, sourceId02);

				Console.WriteLine($"Import progress- Total: {importProgress01.Value.TotalRecords}  Imported: {importProgress01.Value.ImportedRecords}  WithErrors: {importProgress01.Value.ErroredRecords}");
				Console.WriteLine($"Import progress- Total: {importProgress02.Value.TotalRecords}  Imported: {importProgress02.Value.ImportedRecords}  WithErrors: {importProgress02.Value.ErroredRecords}");

				await importJobController.EndAsync(workspaceId, importId);
			}
		}
	}
}
