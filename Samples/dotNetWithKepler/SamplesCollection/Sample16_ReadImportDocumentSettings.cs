// <copyright file="Sample16_ReadImportDocumentSettings.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Builders.Documents;
	using Relativity.Import.V1.Models.Settings;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of reading Import Document Settings for existing job.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample16_ReadImportDocumentSettings()
		{
			// GUID identifiers for import job.
			Guid importId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1031725;

			const int controlNumberColumnIndex = 0;
			const int emailToColumnIndex = 11;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number")
					.WithField(emailToColumnIndex, "Email To"))
				.WithoutFolders();

			using (Relativity.Import.V1.Services.IDocumentConfigurationController documentConfiguration =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IDocumentConfigurationController>())

			using (Relativity.Import.V1.Services.IImportJobController importJobController =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())
			{
				// Create import job.
				Response response = await importJobController.CreateAsync(
					importJobID: importId,
					workspaceID: workspaceId,
					applicationName: "Import-service-sample-app",
					correlationID: "Sample-job-0016");

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Add import document settings to existing import job.
					response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response))
				{
					// Read ImportDocumentSettings of existing import job.
					ValueResponse<ImportDocumentSettings> documentSettings = await documentConfiguration.ReadAsync(workspaceId, importId);

					Console.WriteLine(documentSettings.IsSuccess);
					if (documentSettings.IsSuccess)
					{
						Console.WriteLine(documentSettings.Value.Native.FileNameColumnIndex);
						Console.WriteLine(documentSettings.Value.Other?.ExtractedText?.Encoding);
						Console.WriteLine(documentSettings.Value.Fields.FieldMappings[0].ColumnIndex);
					}
				}
			}
		}
	}
}
