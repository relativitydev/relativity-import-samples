// <copyright file="Sample16_ReadImportDocumentSettings.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetFrameworkClient.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.Samples.NetFrameworkClient.ImportSampleHelpers;
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
			Console.WriteLine($"Running {nameof(Sample16_ReadImportDocumentSettings)}");

			// GUID identifiers for import job.
			Guid importId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1000000;

			const int controlNumberColumnIndex = 0;
			const int emailToColumnIndex = 11;
			const int fileNameColumnIndex = 13;
			const int filePathColumnIndex = 22;

			// Configuration of document import. Builder is used to create the settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number")
					.WithField(emailToColumnIndex, "Email To")
					.WithExtractedTextField(10,
						e => e.WithExtractedTextInSeparateFiles(
								a => a.WithEncoding("UTF-8"))))
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

				ResponseHelper.EnsureSuccessResponse(response, "IImportJobController.CreateAsync");

				// Add import document settings to existing import job.
				response = await documentConfiguration.CreateAsync(workspaceId, importId, importSettings);
				ResponseHelper.EnsureSuccessResponse(response, "IDocumentConfigurationController.CreateAsync");// Read ImportDocumentSettings of existing import job.

				// Read import document settings for particular import job.
				ValueResponse<ImportDocumentSettings> documentSettings = await documentConfiguration.ReadAsync(workspaceId, importId);

				Console.WriteLine(documentSettings.IsSuccess);
				if (documentSettings.IsSuccess)
				{
					// Reading of example fields.
					Console.WriteLine($"Native.FileNameColumnIndex: {documentSettings.Value.Native.FileNameColumnIndex}");
					Console.WriteLine($"ExtractedText.Encoding: {documentSettings.Value.Other?.ExtractedText?.Encoding}");
					Console.WriteLine($"FieldMappings[0].ColumnIndex: {documentSettings.Value.Fields.FieldMappings[0].ColumnIndex}");
				}
			}
		}
	}
}

/* Expected console result:
	Native.FileNameColumnIndex: 13
	ExtractedText.Encoding: UTF-8
	FieldMappings[0].ColumnIndex: 0
*/