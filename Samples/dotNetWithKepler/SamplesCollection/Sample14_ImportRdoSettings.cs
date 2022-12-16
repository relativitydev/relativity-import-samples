﻿// <copyright file="Sample14_ImportRdoSettings.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler.SamplesCollection
{
	using System;
	using System.Threading.Tasks;
	using Relativity.Import.V1;
	using Relativity.Import.V1.Models.Settings;
	using Relativity.Import.V1.Models.Sources;
	using Relativity.Import.V1.Services;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		/// <summary>
		/// Example of creating ImportRdoSettings manually - without ImportRdoSettingsBuilder.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public async Task Sample14_ImportRdoSettings()
		{
			// GUID identifiers for import job and data source.
			Guid importId = Guid.NewGuid();
			Guid sourceId = Guid.NewGuid();

			// destination workspace artifact Id.
			const int workspaceId = 1031725;

			// set of columns indexes in load file used in import settings.
			// example import of Domain RDO.
			const int nameColumnIndex = 0;
			const int domainEmailBccColumnIndex = 2;
			const int domainEmailCcColumnIndex = 3;
			const int domainEmailFromColumnIndex = 4;
			const int domainEmailToColumnIndex = 5;

			// RDO artifact type id
			const int rdoArtifactTypeID = 1000027;

			// Configuration of rdos import. Builder is used to create the settings.
			ImportRdoSettings importSettings = new ImportRdoSettings()
			{
				Overlay = new OverlaySettings
				{
					Mode = OverlayMode.AppendOverlay,
					KeyField = "Name",
					MultiFieldOverlayBehaviour = MultiFieldOverlayBehaviour.UseRelativityDefaults,
				},
				Fields = new FieldsSettings
				{
					FieldMappings = new[]
					{
						new FieldMapping
						{
							Field = "Name",
							ContainsID = false,
							ColumnIndex = nameColumnIndex,
							ContainsFilePath = false,
						},
						new FieldMapping
						{
							Field = "Domains (Email CC)",
							ContainsID = false,
							ColumnIndex = domainEmailCcColumnIndex,
							ContainsFilePath = false,
						},
						new FieldMapping
						{
							Field = "Domain (Email BCC)",
							ContainsID = false,
							ColumnIndex = domainEmailBccColumnIndex,
							ContainsFilePath = false,
						},
						new FieldMapping
						{
							Field = "Domains (Email From)",
							ContainsID = false,
							ColumnIndex = domainEmailFromColumnIndex,
							ContainsFilePath = false,
						},
						new FieldMapping
						{
							Field = "Domains (Email To)",
							ContainsID = false,
							ColumnIndex = domainEmailToColumnIndex,
							ContainsFilePath = false,
						},
					},
				},
				Rdo = new RdoSettings
				{
					ArtifactTypeID = rdoArtifactTypeID,
					ParentColumnIndex = null,
				},
			};

			// Configuration settings for data source. Builder is used to create settings.
			DataSourceSettings dataSourceSettings = new DataSourceSettings
			{
				Type = DataSourceType.LoadFile,
				Path = "C:\\DefaultFileRepository\\samples\\rdo_load_file_02.dat",
				NewLineDelimiter = '#',
				ColumnDelimiter = '|',
				QuoteDelimiter = '^',
				MultiValueDelimiter = '$',
				NestedValueDelimiter = '&',
				Encoding = null,
				CultureInfo = "en-us",
				EndOfLine = DataSourceEndOfLine.Windows,
				FirstLineContainsColumnNames = true,
				StartLine = 0,
			};

			using (Relativity.Import.V1.Services.IRDOConfigurationController rdoConfiguration =
				this._serviceFactory.CreateProxy<Relativity.Import.V1.Services.IRDOConfigurationController>())

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
					correlationID: "Sample-job-import-00014");

				if (this.IsPreviousResponseWithSuccess(response, nameof(IImportJobController.CreateAsync)))
				{
					// Add import rdo settings to existing import job.
					response = await rdoConfiguration.CreateAsync(workspaceId, importId, importSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response, nameof(IImportJobController.CreateAsync)))
				{
					// Add data source settings to existing import job.
					response = await importSourceController.AddSourceAsync(workspaceId, importId, sourceId, dataSourceSettings);
				}

				if (this.IsPreviousResponseWithSuccess(response, nameof(IImportSourceController.AddSourceAsync)))
				{
					// Start import job.
					response = await importJobController.BeginAsync(workspaceId, importId);
				}

				if (this.IsPreviousResponseWithSuccess(response, nameof(IImportJobController.BeginAsync)))
				{
					await this.WaitImportDataSourceToBeCompleted(
						() => importSourceController.GetDetailsAsync(workspaceId, importId, sourceId),
						15000);

					// Get current import progress for specific data source.
					var importProgress = await importSourceController.GetProgressAsync(workspaceId, importId, sourceId);
					var dataSourceState = await importSourceController.GetDetailsAsync(workspaceId, importId, sourceId);

					if (importProgress.IsSuccess)
					{
						Console.WriteLine($"\n\nData source state: {dataSourceState.Value.State}");
						Console.WriteLine($"Import progress: Total records: {importProgress.Value.TotalRecords}, Imported records: {importProgress.Value.ImportedRecords}, Records with errors: {importProgress.Value.ErroredRecords}");
					}
				}

				await importJobController.EndAsync(workspaceId, importId);
			}
		}
	}
}
