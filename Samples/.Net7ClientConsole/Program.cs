// <copyright file="Program.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.Net7Client
{
	using System;
	using System.Threading.Tasks;

	/// <summary>
	/// Console application presenting using Relativity.Import.sdk.models in samples.
	/// </summary>
	internal class Program
	{
		private static void Main(string[] args)
		{

			// Execute samples:
			var sampleCollection = new SampleCollection.ImportServiceSample();
			// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// Uncomment the samples you wish to run:
			// NOTE: Copy sample source files into destination location.
			// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			var task = Task.Run(
				async () =>
				{
					await sampleCollection.Sample01_ImportNativeFiles();

					// await sampleCollection.Sample02_ImportDocumentsInOverlayMode();

					// await sampleCollection.Sample03_ImportFromTwoDataSources();

					// await sampleCollection.Sample04_AddDataSourceToRunningJob();

					// await sampleCollection.Sample05_ImportDocumentsWithExtractedText();

					// await sampleCollection.Sample06_ImportDocumentsToSelectedFolder();

					// await sampleCollection.Sample07_DirectImportSettingsForDocuments();

					// await sampleCollection.Sample08_ImportImages();

					// await sampleCollection.Sample09_ImportProductionFiles();

					// await sampleCollection.Sample10_ImportImagesInAppendOverlayMode();

					//await sampleCollection.Sample11_DirectImportSettingsForImages();

					// await sampleCollection.Sample12_ImportRelativityDynamicObject();

					// await sampleCollection.Sample13_ImportRdoWithParent();

					// await sampleCollection.Sample14_DirectImportSettingsForRdo();

					// await sampleCollection.Sample15_ReadImportRdoSettings();

					// await sampleCollection.Sample16_ReadImportDocumentSettings();

					// await sampleCollection.Sample17_GetImportJobs();

					// await sampleCollection.Sample18_GetDataSource();

					// await sampleCollection.Sample19_GetImportJobDetailsAndProgress();

					// await sampleCollection.Sample20_GetDataSourceDetailsAndProgress();

					// await sampleCollection.Sample21_CancelStartedJob();

					// await sampleCollection.Sample22_ReadResponse();

					// await sampleCollection.Sample23_GetDataSourceErrors();
                });

			try
			{
				task.Wait();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			Console.ReadKey();
		}
	}
}