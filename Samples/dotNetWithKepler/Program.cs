// <copyright file="Program.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler
{
    using System;
    using System.Threading.Tasks;
    using SamplesCollection;

    /// <summary>
    /// Console application presenting using Relativity.Import.sdk in samples.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Provide relativity host, username and password.
            var sample = new ImportServiceSample("https://sample-hosts/", "sample@username", "password!");

            // Execute samples:

            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // Uncomment the samples you wish to run:
            // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            var task = Task.Run(
                async () =>
                {
                    await sample.Sample01_ImportNativeFiles();

                    // await sample.Sample02_ImportDocumentsInOverlayMode();

                    // await sample.Sample03_ImportFromTwoDataSources();

                    // await sample.Sample04_AddDataSourceToRunningJob();

                    // await sample.Sample05_ImportDocumentsWithExtractedText();

                    // await sample.Sample06_ImportDocumentsToSelectedFolder();

                    // await sample.Sample07_ImportDocumentSettingsForNatives();

                    // await sample.Sample08_ImportImages();

                    // await sample.Sample09_ImportProductionFiles();

                    // await sample.Sample10_ImportImagesInAppendOverlayMode();

                    // await sample.Sample11_ImportDocumentSettingsForImages();

                    // await sample.Sample12_ImportRelativityDynamicObject();

                    // await sample.Sample13_ImportRdoWithParent();

                    // await sample.Sample14_ImportRdoSettings();

                    // await sample.Sample15_ReadImportRdoSettings();

                    // await sample.Sample16_ReadImportDocumentSettings();

                    // await sample.Sample17_GetImportJobs();

                    // await sample.Sample18_GetDataSource();

                    // await sample.Sample19_GetImportJobProgress();

                    // await sample.Sample20_GetDataSourceProgress();

                    // await sample.Sample21_CancelStartedJob();

                    // await sample.Sample22_ReadResponse();

                    // await sample.Sample23_GetDataSourceErrors();
                });

            task.Wait();

            Console.ReadKey();
        }
    }
}