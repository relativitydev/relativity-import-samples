# relativity-import
## Introduction
The ***Relativity Import Service API*** provides functionality for importing large numbers of documents, images, and Relativity Dynamic Objects (RDOs) into a Relativity workspace. It provides multiple REST endpoints for programmatically working with import jobs, data sources and their configurations.
Just create import job connected with your workspace where the data should be imported. New import job need to be configured first to define all specific properties related to data import.
Add dataSource or data sources to existing import job defines the source of data tend to be imported. Each data source corresponds to the physical load file (or opticon file) which specified what data and/or external files will be imported.
Starting import job triggers the process that add each added data sources to the queue as separate importing task. All task in queue is automatically scheduled and supervised by system in  background. Jobs and data sources are described by its states and progress that can be queried in their whole lifecycle.





Import Service is available as a RAP application in Relativity One.
### Glossary
**ImportJob** - It is the main object in import service taking part in import flow. It represents single import entity described by its configuration, import state, progress, details and errors.
It aggregates data sources – single import job can consists of many sources.

**DataSource**  - It is an object relates to single set of data to be imported. Each data source has own configuration (e.g. path to load file).
DataSource is described by its state, details, progress and errors.


//tdoo what else? 
// load file : https://help.relativity.com/RelativityOne/Content/Relativity/Relativity_Desktop_Client/Importing/Load_file_specifications.htm
// opticon file
// data set ???
## Import.Service.SDK ###

Import.Service.SDK is a .NET library that provides and simplifies executing import in client application. It contains keplers interfaces for import service.
Import.Service.SDK targets .NET Framework 4.6.2
Import.Service.SDK.Models depends on Import.Service.SDK.Models.

## Import.Service.SDK.Models ###
Import.Service.SDK.Models is a NET library that contains contract models and builder which help prepare payloads in correct and consistent way.
Import.Service.SDK.Models targets .NET Standard 2.0. The NuGet package also includes direct targets for .NET Framework 4.6.2.

### Builders

Builders (**ImportDocumentsSettingsBuilder, ImportRdoSettingsBuilder, DataSourceSettingsBuilder** ) provided in Import.Service.SDK.Models package help create settings for import job and data source in correct and consistent way. It is highly recommended to prepare these objects in such a way in .NET application. They've been implemented in fluent api pattern it is very easy to use them. Moreover using them in client application will avoid the risk of incorrect and inconsistent configuration
which may lead to errors during import process.

Builders are implemented in fluent API manner.

    // Example of creating ImportDocumentSettings with dedicated builder.
    ImportDocumentSettingsBuilder.Create()
                .WithOverlayMode(x => x
					.WithKeyField(overlayKeyField)
					.WithMultiFieldOverlayBehaviour(MultiFieldOverlayBehaviour.MergeAll))
				.WithNatives(x => x
					.WithFilePathDefinedInColumn(filePathColumnIndex)
					.WithFileNameDefinedInColumn(fileNameColumnIndex))
				.WithoutImages()
				.WithFieldsMapped(x => x
					.WithField(controlNumberColumnIndex, "Control Number")
					.WithExtractedTextField(extractedTextPathColumnIndex, e => e
						.WithExtractedTextInSeparateFiles(f => f
							.WithEncoding("UTF-8"))))
				.WithFolders(f => f
					.WithRootFolderID(rootFolderId, r => r
						.WithFolderPathDefinedInColumn(folderPathColumnIndex)));


     // Example of creating DataSourceSettings with dedicated builder.
			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
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


## Getting Started ##
Relativity Import API provides set of RESTful endpoints. Each import (import documents, import images, import productions, import rdos) requires the calls to the endpoints in specific order. 
In general there are two ways to work with provided API:
 - As an ordinary REST service - sending a http request from any http client in any technology
 In case of using .NET the Import.Service.SDK.Models package can be used (mentioned above).

        HttpClient client = new HttpClient();
        //...
        var response = await httpClient.PostAsJsonAsync(createImportJobUri,createJobPayload);

  Please investigate dedicated code samples for .NET 7 or for PowerShell scripts.

- As Kepler service - using Kepler's Proxy from Kepler framework and invoking endpoints as ordinary methods in client application. To do this Import.Service.SDK need to be installed in client application.

        using (Relativity.Import.V1.Services.IImportJobController importJobController =_serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>()){

            // Create import job.
            Response response = await importJobController.CreateAsync(
                importJobID: importId,
                workspaceID: workspaceId,
                applicationName: "Import-service-sample-app",
                correlationID: "Sample-job-0001");
            }
        }

    Please investigate dedicated code samples for .NET 4.6.2 with Kepler.
### **Installing via NuGet**  // DL legacy dla podgladu narazie !
[![Version](https://img.shields.io/nuget/v/Relativity.DataExchange.Client.SDK.svg?color=royalblue)](https://www.nuget.org/packages/Relativity.DataExchange.Client.SDK)
[![Downloads](https://img.shields.io/nuget/dt/Relativity.DataExchange.Client.SDK?color=green)](https://www.nuget.org/packages/Relativity.DataExchange.Client.SDK)

Install-Package Import.Service.SDK 

[![Version](https://img.shields.io/nuget/v/Relativity.DataExchange.Client.SDK.svg?color=royalblue)](https://www.nuget.org/packages/Relativity.DataExchange.Client.SDK)
[![Downloads](https://img.shields.io/nuget/dt/Relativity.DataExchange.Client.SDK?color=green)](https://www.nuget.org/packages/Relativity.DataExchange.Client.SDK)

Install-Package Import.Service.SDK.Models

### Authorization
> non-kepler application

Import Service API conforms to the same authentication rules as the other Relativity REST APIs. 

The more details can be find under the following link:
[REST_API_authentication](https://platform.relativity.com/RelativityOne/Content/REST_API/REST_API_authentication.htm)

> kepler application

The Kepler framework uses a proxy to handle client requests. The more details can be find under the following link:
[Proxies_and_authentication](https://platform.relativity.com/RelativityOne/Content/Kepler_framework/Proxies_and_authentication.htm#Service)

### Permissions
The following Relativity permissions are required to use importing features in Import Service API.

| Object Security section| Permission |
| :---- | :----: |
| •	Document |	View, Add, Edit |
| •	Relativity Import Job: View, Add, Edit |	View, Add, Edit |
| •	Relativity Import Data Source |	View, Add, Edit |

| Tab Visibility|
| :---- |
| •	Documents |

| Admin Operation|
| :---- |
| •	Allow Import |

## General Import flow
The general flow includes several steps consisted in sending appropriate http request.

1. **Create Import Job** 

   Creates import job entity in particular workspace. Job is defined by its unique Id provided in the request which is used in the next steps.

2. **Configure Import Job**
  Configures existing import job by defining sets of significant parameters including import type, its mode, field mapping.
  
3. **Add one or multiple DataSources**  (optional)
  Creating data source entity (sets of its configuration) or entities for particular import job. Data source is identified by its unique Id provided in the request. Data source configuration includes path to “load file” and other significant parameters telling how data in load file will be read and interpreted by system.

   Many data sources can be added to the same import job. Data sources can be added both before job is started and after.


4. **Begin Job**  
  Starts Import Job which enables the process of executing import data to workspace based on the configuration assigned in previous steps.
 
5. **Add one or multiple DataSources**  (optional)  

6. **End Import Job**  (optional)
  Ends import job that was already started. It is optional step but it is highly recommended in case when no more data source is plan to be added for particular job.



### Import Documents
1. **Create Import Job** 

2. **Create Import Job Configuration**

        ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
            .WithAppendMode()
            .WithNatives(x => x
                .WithFilePathDefinedInColumn(filePathColumnIndex)
                .WithFileNameDefinedInColumn(fileNameColumnIndex))
            .WithoutImages()
            .WithFieldsMapped(x => x
                .WithField(controlNumberColumnIndex, "Control Number")
                .WithField(custodianColumnIndex, "Custodian")
                .WithField(emailToColumnIndex, "Email To")
                .WithField(dateSentColumnIndex, "Date Sent"))
            .WithoutFolders();

3. **Add DataSource (relates to load file)** 

			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForLoadFile("//fileshare//path//load_file.dat)
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


4. **Begin Job** 
5. **End Import Job**  (optional)


### Import Images
1. **Create Import Job** 

        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/00000000-0000-0000-0000-000000000000/'
        -H 'X-CSRF-Header: -' 
        -d '{
        "applicationName": "simpleImport",
        "correlationID": "c0r31ati0n_ID"
        }'

2. **Create Import Job Configuration**

			// Configuration settings for images import. Builder is used to create settings.
			ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
				.WithAppendMode()
				.WithoutNatives()
				.WithImages(i => i
					.WithAutoNumberImages()
					.WithoutProduction()
					.WithoutExtractedText()
					.WithFileTypeAutoDetection())
				.WithoutFieldsMapped()
				.WithoutFolders();

3. **Add DataSource (related to opticon file)** 

			// Configuration settings for data source. Builder is used to create settings.
			// ForOpticonFile("//fileshare//path/opticon_file.opt") is used when importing images.
			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForOpticonFile(opticonFilePath)
				.WithDefaultDelimitersForOpticonFile()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();


4. **Begin Job** 
5. **End Import Job**  (optional)


### Import Relativity Dynamic Objects (RDO)
1. **Create Import Job** 

2. **Create Import Job Configuration**

        // Configuration RDO settings for Relativity Dynamic Objects (RDOs) import. Builder is used to create settings.
        ImportRdoSettings importSettings = ImportRdoSettingsBuilder.Create()
            .WithAppendMode()
            .WithFieldsMapped(f => f
                .WithField(nameColumnIndex, "Name")
            .WithRdo(r => r
                .WithArtifactTypeId(domainArtifactTypeID)
                .WithoutParentColumnIndex());

3. **Add DataSource (related to opticon file)** 

		// Configuration settings for data source. Builder is used to create settings.
			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForLoadFile("loadfile.dat")
				.WithDefaultDelimiters()
				.WithFirstLineContainingHeaders()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();


4. **Begin Job** 
5. **End Import Job**  (optional)

# ImportDocumentSettings Description

| Property                      | Description                                                                                                          |
| ------------------------------| ---------------------------------------------------------------------------------------------------------------------|
| Overlay                        | Overlay mode settings. If not set Append mode is used.                                  |
| Overlay.Mode                         | Defineds the overlay mode (Overlay, AppendOverlay).                                  |
| Overlay.KeyField                     | The Relativity field that is used to overlay records.                                                   |
| Overlay.MultiFieldOverlayBehaviour   | The behaviour for overlay imports with multiple choice and multiple object fields.                      |
| NativeSettings                | The native file settings. If not set, no native files would be imported.                                        |
| FilePathColumnIndex           | the index of the column in the data source (count from zero) which contains path to the native file                   |
| FileNameColumnIndex            | The index of the column in the data source (count from zero) which contains native file name metadata. If property is not set, value is retrieved from file.              |
| ImageSettings                 | The total number of transferred metadata bytes.                                                                      |
| PageNumbering                  | Enum Value indicating whether a page number is automatically appended to a page-level.identifier.                                                                                        |
| ProductionID                     | The valid ArtifactID for a existing production set.                       |
| LoadExtractedText                | value indicating whether Extracted Text should be loaded together with images if Extracted Text file is available.|
| FileType                     | Value indicating whether Automatic detection of file type is used or file types are specified by user as images of PDF.|
| ProductionID                     | The valid ArtifactID for a existing production set.                       |

## Responses

Each response to POST requests has unified schema:

        {
        "IsSuccess": true,
        "ErrorMessage": "",
        "ErrorCode": "",
        "ImportJobID": "00000000-0000-0000-0000-000000000000"
        }

Each response to GET requests has unified schema:

        {
        "Value": {
            "RecordsInDataSources": 11,
            "ImportedRecords": 10,
            "ErrorRecords": 1
        },
        "IsSuccess": true,
        "ErrorMessage": "",
        "ErrorCode": "",
        "ImportJobID": "00000000-0000-0000-0000-000000000000"
        }

# Error Codes

// todo: list of error codes

// Details models

# Samples

## Samples types and structure
There are three types of sample application that presents the using of import service API features.
- .Net7ConsoleClient: C# console application  (targets .NET 7)
- .KeplerClientConsole: C# console application (targets .NET 4.6.2 and using Kepler client)
- Rest Samples: sets of powershell scripts

Sample structure
- Each sample import flow is presented in separate file (e.g. [Sample04_AddDataSourceToRunningJob.cs](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample04_AddDataSourceToRunningJob.cs).
- Each code file/class with sample flow is numbered
- All samples for each application are consistent. e.g. Sample 08 presenting the same import flow  in Net7ConsoleClient, KeplerClientConsole and powerhsell script.
- Sample code contains comments related to invoking endpoints
- Sample code contains expected result for each presenting import flow. 
- Samples code contains also sets of data (natives, images, loadfiles)


List of samples:
| Sample name | .Net with Kepler| .NET | PowerShell |
| :---- | :----: | :----: | :----: |
 | Sample01_ImportNativeFiles | [Sample01](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample01_ImportNativeFiles.cs) | [Sample01](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample01_ImportNativeFiles.cs) | [Sample01](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample01-import-native-files.ps1) | 
 | Sample02_ImportDocumentsInOverlayMode | [Sample02](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample02_ImportDocumentsInOverlayMode.cs) | [Sample02](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample02_ImportDocumentsInOverlayMode.cs) | [Sample02](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample02-import-documents-in-overlay-mode.ps1) | 
| Sample03_ImportFromTwoDataSources | [Sample03](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample03_ImportFromTwoDataSources.cs) | [Sample03](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample03_ImportFromTwoDataSources.cs) | [Sample03](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample03-import-from-two-data-sources.ps1) | 
| Sample04_AddDataSourceToRunningJob|  [Sample04](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample04_AddDataSourceToRunningJob.cs) | [Sample04](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample04_AddDataSourceToRunningJob.cs) | [Sample04](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample04-add-data-source-to-running-job.ps1) | 
| Sample05_ImportDocumentsWithExtractedText | [Sample05](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample05_ImportDocumentsWithExtractedText.cs) | [Sample05](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample05_ImportDocumentsWithExtractedText.cs) | [Sample05](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample05-import-documents-with-extracted-text.ps1) | 
| Sample06_ImportDocumentsToSelectedFolder | [Sample06](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample06_ImportDocumentsToSelectedFolder.cs) | [Sample06](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample06_ImportDocumentsToSelectedFolder.cs) | [Sample06](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample06-import-documents-to-selected-folder.ps1) | 
| Sample07_ImportDocumentSettingsForNatives | [Sample07](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample07_ImportDocumentSettingsForNatives.cs) | [Sample07](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample07_ImportDocumentSettingsForNatives.cs) | [Sample07](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample07-import-document-settings-for-natives.ps1) | 
| Sample08_ImportImages | [Sample08](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample08_ImportImages.cs) | [Sample08](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample08_ImportImages.cs) | [Sample08](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample08-import-images.ps1) | 
 | Sample09_ImportProductionFiles | [Sample09](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample09_ImportProductionFiles.cs) | [Sample09](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample09_ImportProductionFiles.cs) | [Sample09](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample09-import-production-files.ps1) | 
 | Sample10_ImportImagesInAppendOverlayMode |[Sample10](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample10_ImportImagesInAppendOverlayMode.cs) | [Sample10](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample10_ImportImagesInAppendOverlayMode.cs) | [Sample10](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample10-import-images-in-append-overlay-mode.ps1) | 
 | Sample11_ImportDocumentSettingsForImages| [Sample11](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample11_ImportDocumentSettingsForImages.cs) | [Sample11](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample11_ImportDocumentSettingsForImages.cs) | [Sample11](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample11-import-document-settings-for-images.ps1) | 
 | Sample12_ImportRelativityDynamicObject | [Sample12](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample12_ImportRelativityDynamicObject.cs) | [Sample12](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample12_ImportRelativityDynamicObject.cs) | [Sample12](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample12-import-relativity-dynamic-object.ps1) | 
 | Sample13_ImportRdoWithParent | [Sample13](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample13_ImportRdoWithParent.cs) | [Sample13](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample13_ImportRdoWithParent.cs) | [Sample13](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample13-import-rdo-with-parent.ps1) | 
 | Sample14_ImportRdoSettings | [Sample14](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample14_ImportRdoSettings.cs) | [Sample14](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample14_ImportRdoSettings.cs) | [Sample14](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample14-import-rdo-settings.ps1) | 
 | Sample15_ReadImportRdoSettings|  [Sample15](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample15_ReadImportRdoSettings.cs) | [Sample15](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample15_ReadImportRdoSettings.cs) | [Sample15](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample15-read-import-rdo-settings.ps1) | 
 | Sample16_ReadImportDocumentSettings | [Sample16](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample16_ReadImportDocumentSettings.cs) | [Sample16](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample16_ReadImportDocumentSettings.cs) | [Sample16](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample16-read-import-document-settings.ps1) | 
 | Sample17_GetImportJobs | [Sample17](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample17_GetImportJobs.cs) | [Sample17](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample17_GetImportJobs.cs) | [Sample17](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample17-read-import-jobs.ps1) | 
 | Sample18_GetDataSource | [Sample18](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample18_GetDataSource.cs) | [Sample18](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample18_GetDataSource.cs) | [Sample18](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample18-get-data-source.ps1) | 
 | Sample19_GetImportJobDetailsAndProgress | [Sample19](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample19_GetImportJobDetailsAndProgress.cs) | [Sample19](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample19_GetImportJobDetailsAndProgress.cs) | [Sample19](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample19-get-import-job-details-and-progress.ps1) | 
 | Sample20_GetDataSourceDetailsAndProgress | [Sample20](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample20_GetDataSourceDetailsAndProgress.cs) | [Sample20](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample20_GetDataSourceDetailsAndProgress.cs) | [Sample20](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample20-get-data-source-details-and-progress.ps1) | 
 | Sample21_CancelStartedJob | [Sample21](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample21_CancelStartedJob.cs) | [Sample21](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample21_CancelStartedJob.cs) | [Sample21_](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample21-cancel-started-job.ps1) | 
 | Sample22_ReadResponse | [Sample22](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample22_ReadResponse.cs) | [Sample22](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample22_ReadResponse.cs) | [Sample22](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample22-read-response.ps1) | 
 | Sample23_GetDataSourceErrors | [Sample23](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample23_GetDataSourceErrors.cs) | [Sample23](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample23_GetDataSourceErrors.cs) | [Sample23](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample23-get-data-source-errors.ps1) | 
 
## .NET 7 Code Samples

 
 To run a sample code:
 
 - Copy first the content of [sampeData](https://github.com/relativitydev/relativity-import-samples/tree/main/SampleDataSources) to your Relativity fileshare.
 - Uncomment line with sample invocation you want to run in Main method.  

            // await sampleCollection.Sample08_ImportImages();
            // await sampleCollection.Sample09_ImportProductionFiles();

            await sampleCollection.Sample10_ImportImagesInAppendOverlayMode();

 - Set the proper credentials and URI of your Relativity instance in [RelativityUserSettings](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/Helpers/RelativityUserSettings.cs) helper calls.

        public class RelativityUserSettings
        {
            public const string BaseAddress = "https://host/Relativity.REST/";

            public const string UserName = "sample.user@test.com";

            public const string Password = "password!";
        }

 - Update workspaceId const to the proper value equals Id of the workspace where you intend to import data. It is required in each sample. 

 
        destination workspace artifact Id.
        const int workspaceId = 1000000;

 - Updated other Ids related to your workspace - productionSetsArtifactId , rootFolderId,rdoArtifactTypeID. They are required only by specific samples.
 - Update const which defines the path to the load file (e.g. const string  loadFile01Path) according to the location where you copied sample data.

 
			// Path to the file in opticon format used in data source settings.
			const string opticonFilePath = "X:\\DefaultFileRepository\\samples\\opticon_01.opt";

 - Run application 



## KeplerClient Code Samples
 
 To run a sample code:
 
 - Copy first the content of [sampeData](https://github.com/relativitydev/relativity-import-samples/tree/main/SampleDataSources) to your Relativity fileshare.
 - Uncomment line with sample invocation you want to run in Main method.
 - Set the proper credentials and host address of your Relativity instance in [RelativityUserSettings](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/ImportSampleHelpers/RelativityUserSettings.cs) helper calls.
 - Update workspaceId const to the proper value equal Id of the workspace where you intend to import data. It is required in each sample. 
 - Updated other Ids related to your workspace - productionSetsArtifactId , rootFolderId,rdoArtifactTypeID. They are required only by specific samples.
 - Update const which defines the path to the load file (e.g. const string  loadFile01Path) according to the location where you copied sample data.
 - Run application 

## Powershell Script Samples
 
 To run a sample code:
 
 - Copy first the content of [sampeData](https://github.com/relativitydev/relativity-import-samples/tree/main/SampleDataSources) to your Relativity fileshare.

- Uncomment line with sample invocation you want to run in [run-sample-Import.ps1](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/run-sample-Import.ps1).

        Describe "Sample import" {
        . "$global:rootDir\SamplesCollection\sample01-import-native-files.ps1"

        # . "$global:rootDir\SamplesCollection\sample02-import-documents-in-overlay-mode.ps1"

 - Set the proper credentials and host address of your Relativity instance in "run-sample-Import.ps1".

        $hostAddress = "https://sample-host/"
        $userName = "sample@username"
        $password = "password!"

 - Update workspaceId  to the proper value equal Id of the workspace where you intend to import data. It is required in each sample. 

- Update variable which defines the path to the load file/opticon file (e.g. $opticonFilePath) according to the location where you copied sample data.

        $workspaceId = 1000000
        $loadFilePath = "C:\DefaultFileRepository\samples\load_file_01.dat"

 - Updated other Ids related to your workspace - productionSetsArtifactId , rootFolderId,rdoArtifactTypeID. They are required only by specific samples.

 - Invoke run-sample-Import.ps1

### REST API
Create Import Job

        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/00000000-0000-0000-0000-000000000000/'
        -H 'X-CSRF-Header: -' 
        -d '{
        "applicationName": "simpleImport",
        "correlationID": "c0r31ati0n_ID"
        }'
    
Begin Import Job

    curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/77140fb9-f515-4b65-a2ce-c347492e2905/begin/' 
    -H 'X-CSRF-Header: -' 
    -d ''

End Import Job

     curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/77140fb9-f515-4b65-a2ce-c347492e2905/end/'
    -H 'X-CSRF-Header: -' 
    -d ''

Get Import Job Progress

    curl -X 'GET' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/00000000-0000-0000-0000-000000000000/progress/' 
    -H 'X-CSRF-Header: -'

Get Import Job Details

    curl -X 'GET' 'https://relativit-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/00000000-0000-0000-0000-000000000000/details/' 
    -H 'X-CSRF-Header: -'

Create Import Job Configuration

    curl -X 'POST' \'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/00000000-0000-0000-0000-000000000000/documents-configurations/' 
    -H 'X-CSRF-Header: -' 
    -d '{ "importSettings": ...}'

Configuration payload example:

    {
    "importSettings": {
        "Overlay": {
        "Mode": "Overlay",
        "KeyField": "KeyField",
        "MultiFieldOverlayBehaviour": "UseRelativityDefaults"
        },
        "Native": null,
        "Image": {
        "ProductionID": 1003663,
        "PageNumbering": "AutoNumberImages",
        "LoadExtractedText": true
        },
        "Fields": {
        "FieldMappings": [
            {
            "ColumnIndex": 4,
            "Field": "one",
            "ContainsID": false,
            "ContainsFilePath": false
            }
        ]
        },
        "Folder": {
        "RootFolderID": 1003663,
        "FolderPathColumnIndex": 2
        }
    }
    }'


Create Data Source

    curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/00000000-0000-0000-0000-000000000000/sources/00000000-0000-0000-0000-000000000000'
    -H 'X-CSRF-Header: -' \
    -H 'Content-Type: application/json' 
    -d '{ dataSourceSettings }
    }'
dataSourceSettings payload example

    {
    "dataSourceSettings": {
        "Path": "C:\\tmp_loadfile\\loadfile.dat",
        "EndOfLine": "Windows",
        "Type": "Opticon",
        "FirstLineContainsColumnNames": true,
        "StartLine": 0,
        "ColumnDelimiter": "|",
        "QuoteDelimiter": "^",
        "NewLineDelimiter": "#",
        "MultiValueDelimiter": "$",
        "NestedValueDelimiter": "&",
        "Encoding": "utf-8",
        "CultureInfo": "en-US"
    }
    }'

Data source details

    curl -X 'GET' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/00000000-0000-0000-0000-000000000000/sources/00000000-0000-0000-0000-000000000000/details' 
    -H 'X-CSRF-Header: -'

Data source progress

    curl -X 'GET' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/00000000-0000-0000-0000-000000000000/sources/00000000-0000-0000-0000-000000000000/progress' 
    -H 'X-CSRF-Header: -'


Data source Item errors

    curl -X 'GET' 'https://relativity.roadie.so/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/00000000-0000-0000-0000-000000000000/sources/00000000-0000-0000-0000-000000000000/itemerrors?start=0&length=10'
    -H 'accept: application/json' 
    -H 'X-CSRF-Header: -'

