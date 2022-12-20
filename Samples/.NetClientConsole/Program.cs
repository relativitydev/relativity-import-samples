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
			// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// Uncomment the samples you wish to run:
			// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			var task = Task.Run(
				async () =>
				{
					 await sampleCollection.Sample01_ImportNativeFiles();

					// await sampleCollection.Sample02_ImportDocumentsInOverlayMode();

					//await sampleCollection.Sample03_ImportFromTwoDataSources();

					//await sampleCollection.Sample04_AddDataSourceToRunningJob();

					// await sampleCollection.Sample05_ImportDocumentsWithExtractedText();

					await sampleCollection.Sample06_ImportDocumentsToSelectedFolder();

					// await v.Sample07_ImportDocumentSettingsForNatives();

					// await sampleCollection.Sample08_ImportImages();

					// await sampleCollection.Sample09_ImportProductionFiles();

					// await sampleCollection.Sample10_ImportImagesInAppendOverlayMode();

					// await sampleCollection.Sample11_ImportDocumentSettingsForImages();

					// await sampleCollection.Sample12_ImportRelativityDynamicObject();

					// await sampleCollection.Sample13_ImportRdoWithParent();

					// await sampleCollection.Sample14_ImportRdoSettings();

					//await sampleCollection.Sample15_ReadImportRdoSettings();

					// await sampleCollection.Sample16_ReadImportDocumentSettings();

					// await sampleCollection.Sample17_GetImportJobs();

					// todo await sampleCollection.Sample18_GetDataSource();

					// todo await sampleCollection.Sample19_GetImportJobProgress();

					// todo await sampleCollection.Sample20_GetDataSourceProgress();

					// todo await sampleCollection.Sample21_CancelStartedJob();

					// todo await sampleCollection.Sample22_ReadResponse();

					// todo await sampleCollection.Sample23_GetDataSourceErrors();
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