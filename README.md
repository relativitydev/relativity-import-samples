# relativity-import

### Table of Contents
**[Introduction](#introduction)**<br>
**[Prerequisites](#prerequisites)**<br>
**[Glossary](#glossary)**<br>
**[Builders](#builders)**<br>
**[Getting Started](#getting-started)**<br>
**[Authorization](#authorization)**<br>
**[Permissions](#prermisions)**<br>
**[General Import flow ](#prermisions)**<br>
**[Import documents flow](#import-documents-flow)**<br>
**[Import images flow](#import-images-flow)**<br>
**[Import Rdo flow](#import-rdos-flow)**<br>
**[Responses description](#responses-description)**<br>
**[Error Codes](#error-codes)**<br>
**[Samples](#samples)**<br>
#### *[.NET 7 Console Application](#net-7-code-samples)*<br>
#### *[.NET Framework & Kepler Console Application](#keplerclient-code-samples)*<br>
#### *[Powershell scripts](#powershell-script-samples)*<br>
**[Error Codes](#error-codes)**<br>
**[Open API](#rest-api)**<br>


## Introduction
The ***Relativity Import Service API*** provides functionality for importing large numbers of documents, images, and Relativity Dynamic Objects (RDOs) into a Relativity workspace. 
// todo: need to be redacted
Just create import job connected with your workspace where the data should be imported. New import job need to be configured first to define all specific properties related to data import.
Add dataSource or data sources to existing import job defines the source of data tend to be imported. Each data source corresponds to the physical load file (or opticon file) which specified what data and/or external files will be imported.
Starting import job triggers the process that add each added data sources to the queue as separate importing task. All task in queue is automatically scheduled and supervised by system in  background. Jobs and data sources are described by its states and progress that can be queried in their whole lifecycle.It provides multiple REST endpoints for programmatically working with import jobs, data sources and their configurations.

Import Service is available as a RAP application in Relativity One.


# Prerequisites

1. Thew following relativity application need to be installed:
- *Import*  - installed in Relativity workspace
- *DataTransfer.Legacy*  - installed in Relativity instance scope.

2. Appropriate user [permissions](#permissions) need to be set.
3. Data set - load files, source files (native documents, images, text files) - need to be located on the destination file share.
## Glossary
**ImportJob** - It is the main object in import service taking part in import flow. It represents single import entity described by its configuration, import state, progress, details and errors.
It aggregates data sources – single import job can consists of many sources.

**DataSource**  - It is an object relates to single set of data to be imported. Each data source has own configuration (e.g. path to load file).
DataSource is described by its state, details, progress and errors.


## Builders

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
### Import.Service.SDK ###

Import.Service.SDK is a .NET library that contains keplers interfaces for import service.
It provides and simplifies executing import in client application.
Import.Service.SDK targets .NET Framework 4.6.2

**NOTE: Use this package when your application USE keplers.**
### **Installing via NuGet** 

[![Version](https://img.shields.io/nuget/v/Import.Service.SDK.svg?color=royalblue)](https://www.nuget.org/packages/Import.Service.SDK)
[![Downloads](https://img.shields.io/nuget/dt/Import.Service.SDK?color=green)](https://www.nuget.org/packages/Import.Service.SDK)

Install-Package Import.Service.SDK 

## Import.Service.SDK.Models ###
Import.Service.SDK.Models is a NET library that contains contract models for API and [builders](#builders) which help user to prepare payloads in correct and consistent way.
Import.Service.SDK.Models targets .NET Standard 2.0. The NuGet package also includes direct targets for .NET Framework 4.6.2
**NOTE: Use this standalone package when your application does not use keplers.**

[![Version](https://img.shields.io/nuget/v/Import.Service.SDK.Models.svg?color=royalblue)](https://www.nuget.org/packages/Import.Service.SDK.Models)
[![Downloads](https://img.shields.io/nuget/dt/Import.Service.SDK.Models?color=green)](https://www.nuget.org/packages/Import.Service.SDK.Models)

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

### Simple Import Documents Example flow
1.  **Create Import Job** 
        
    > curl
    
         curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/e694ad62-198d-4ecb-936d-1862ddfa4235"
        -H 'X-CSRF-Header: -' 
        -d '{
        "applicationName": "simpleImportDocuments",
        "correlationID": "c0r31ati0n_ID"
        }'
<br>

2. **Create Import Job Configuration** 

    > curl

        curl -X 'POST' \'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/e694ad62-198d-4ecb-936d-1862ddfa4235/documents-configurations/' 
        -H 'X-CSRF-Header: -' 
        -d "$importSettingsPayloadJson"


    Import Configuration payload example:

    > JSON

        {
        "importSettings": {
            "Overlay":null,
            "Native":{
                "FilePathColumnIndex": "22",
                "FileNameColumnIndex": "13"
            },
            "Image": null,
            "Fields": {
                "FieldMappings": [
                    {
                    "ColumnIndex": 0,
                    "Field": "Control Number",
                    "ContainsID": false,
                    "ContainsFilePath": false
                    },
            ]
            },
            "Folder": {
                "RootFolderID": 1003663,
                "FolderPathColumnIndex": 2
            }
        }
        }'

    > C#  Builders

        ImportDocumentSettings importSettings = ImportDocumentSettingsBuilder.Create()
            .WithAppendMode()
            .WithNatives(x => x
                .WithFilePathDefinedInColumn(filePathColumnIndex)
                .WithFileNameDefinedInColumn(fileNameColumnIndex))
            .WithoutImages()
            .WithFieldsMapped(x => x
                .WithField(controlNumberColumnIndex, "Control Number")
            .WithFolders(f => f
                .WithRootFolderID(rootFolderId, r => r
                    .WithFolderPathDefinedInColumn(folderPathColumnIndex)));

    > C# 

        ImportDocumentSettings importSettings = new ImportDocumentSettings()
			{
				Overlay = null,
				Native = new NativeSettings
				{
					FileNameColumnIndex = fileNameColumnIndex,
					FilePathColumnIndex = filePathColumnIndex,
				},
				Fields = new FieldsSettings
				{
					FieldMappings = new[]
					{
						new FieldMapping
						{
							Field = "Control Number",
							ContainsID = false,
							ColumnIndex = 0,
							ContainsFilePath = false,
						},
					},
				},
				Folder = new FolderSettings
				{
					FolderPathColumnIndex = folderPathColumnIndex,
					RootFolderID = 1003663,
				},
				Other = null,
			};

<br>

3. **Add DataSource** 
    > curl

        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/e694ad62-198d-4ecb-936d-1862ddfa4235/sources/0cb922a2-8df4-42fd-9429-c241410a0d1e'
        -H 'X-CSRF-Header: -' \
        -H 'Content-Type: application/json' 
        -d "$dataSourceSettingsJson"
        }'

    Data source configuration payload example:

    > JSON

        {
        "dataSourceSettings": {
            "Path": "//fileshare//path//load_file.dat",
            "FirstLineContainsColumnNames": true,
            "StartLine": 1,
            "ColumnDelimiter": "|",
            "QuoteDelimiter": "^",
            "NewLineDelimiter": "#",
            "MultiValueDelimiter": ";",
            "NestedValueDelimiter": "&",
            "EndOfLine" = 0
             Encoding" = null
            "CultureInfo" : "en-US",
            "Type": 2
            }
        }'

    > C# & builders
   
			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForLoadFile("//fileshare//path//load_file.dat)
				.WithDelimiters(d => d
					.WithColumnDelimiters('|')
					.WithQuoteDelimiter('^')
					.WithNewLineDelimiter('#')
					.WithNestedValueDelimiter('&')
					.WithMultiValueDelimiter(';'))
				.WithFirstLineContainingHeaders()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();

    > C#

			DataSourceSettings dataSourceSettings = new DataSourceSettings
			{
				Type = DataSourceType.LoadFile,
				Path = "//fileshare//path//load_file.dat",
				NewLineDelimiter = '#',
				ColumnDelimiter = '|',
				QuoteDelimiter = '^',
				MultiValueDelimiter = ';',
				NestedValueDelimiter = '&',
				Encoding = null,
				CultureInfo = "en-us",
				EndOfLine = DataSourceEndOfLine.Windows,
				FirstLineContainsColumnNames = true,
				StartLine = 0,
			};

<br>

4. **Begin Job**
    > curl

        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/e694ad62-198d-4ecb-936d-1862ddfa4235/begin/' 
        -H 'X-CSRF-Header: -' 
        -d ''

<br>

5. **End Import Job**  (optional)

    > curl
        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/e694ad62-198d-4ecb-936d-1862ddfa4235/end/'
        -H 'X-CSRF-Header: -' 
        -d ''



<br><br>

# Import Images
1. **Create Import Job** 
    > curl

        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/'
        -H 'X-CSRF-Header: -' 
        -d '{
        "applicationName": "simpleImportImages",
        "correlationID": "img0r22ati0n_ID"
        }'

2. **Create Import Job Configuration**
    > curl

        curl -X 'POST' \'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/documents-configurations/' 
        -H 'X-CSRF-Header: -' 
        -d "$importSettings"

    Import Configuration payload example:

    >JSON

        {
        "importSettings": {
            "Overlay":null,
            "Native":null,
        "Image": 
            {
                "PageNumbering": 1,
                "ProductionID": null,
                "LoadExtractedText": false,
                "FileType": 0    
            }
            "Fields": null,
            "Folder": null
            }
        }'

    >  C# Builder

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
            

3. **Add DataSource** 

    > curl

            curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/sources/0cb922a2-8df4-42fd-9429-c241410a0002'
            -H 'X-CSRF-Header: -' \
            -H 'Content-Type: application/json' 
            -d "$dataSourceSettingsJson"
            }'

    > JSON
    
        {
        "dataSourceSettings": {
            "Path": "//fileshare//path//opticon_file.opt",
            "FirstLineContainsColumnNames": false,
            "StartLine": 0;
            "EndOfLine" = 0
             Encoding" = null
            "CultureInfo" : null,
            "Type": 1
            }
        }'


    > C# Builders

			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForOpticonFile("//fileshare//path//opticon_file.opt")
				.WithDefaultDelimitersForOpticonFile()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();

    > C#

			DataSourceSettings dataSourceSettings = new DataSourceSettings
			{
				Type = DataSourceType.Opticon,
				Path = "//fileshare//path//opticon_file.opt",
				NewLineDelimiter = default,
				ColumnDelimiter = default,
				QuoteDelimiter = default,
				MultiValueDelimiter = default,
				NestedValueDelimiter = default,
				Encoding = null,
				CultureInfo = null,
				EndOfLine = DataSourceEndOfLine.Windows,
				FirstLineContainsColumnNames = false,
				StartLine = 0,
			};

<br>

4. **Begin Job** 
    > curl

        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/begin/' 
        -H 'X-CSRF-Header: -' 
        -d ''

5. **End Import Job**  (optional)
    > curl

        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/end/'
        -H 'X-CSRF-Header: -' 
        -d ''

<br><br>
## Import Relativity Dynamic Objects (RDO)

1. **Create Import Job** 
    > curl
    
        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/77140fb9-f515-4b65-a2ce-c347492e2905/'
        -H 'X-CSRF-Header: -' 
        -d '{
        "applicationName": "simpleImportRdo",
        "correlationID": "rdor31ati0n_ID"
        }'

2. **Create Import Job Configuration**

    > curl

        curl -X 'POST' \'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/77140fb9-f515-4b65-a2ce-c347492e2905/documents-configurations/' 
        -H 'X-CSRF-Header: -' 
        -d $"importRdoSettings"'


    Import RDO Configuration payload example:

     > JSON

        {
            "importSettings": {
                "OverwriteMode": "Append",
                "Fields": {
                "FieldMappings": [
                    {
                    "ColumnIndex": 0,
                    "Field": "Name"
                    },
                    ]
                },
                "Rdo": {
                    "ArtifactTypeID": 1000066,
                    "ParentColumnIndex" :null
                },
            }
        }

    > C# Builder
        
        ImportRdoSettings importSettings = ImportRdoSettingsBuilder.Create()
            .WithAppendMode()
            .WithFieldsMapped(f => f
                .WithField(nameColumnIndex, "Name")
            .WithRdo(r => r
                .WithArtifactTypeId(domainArtifactTypeID)
                .WithoutParentColumnIndex());

    > C# 

        ImportRdoSettings importSettings = new ImportRdoSettings()
			{
				Overlay = null,
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
					},
				},
				Rdo = new RdoSettings
				{
					ArtifactTypeID = rdoArtifactTypeID,
					ParentColumnIndex = null,
				},
			};


3. **Add DataSource (related to opticon file)** 
    > curl

        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/sources/0cb922a2-8df4-42fd-9429-c241410a0002'
        -H 'X-CSRF-Header: -' \
        -H 'Content-Type: application/json' 
        -d "$dataSourceSettingsJson"
        }'

    Data source configuration payload example:

    > JSON

        {
        "dataSourceSettings": {
            "Path": "//fileshare//path//load_file.dat",
            "FirstLineContainsColumnNames": true,
            "StartLine": 1,
            "ColumnDelimiter": "|",
            "QuoteDelimiter": "^",
            "NewLineDelimiter": "#",
            "MultiValueDelimiter": ";",
            "NestedValueDelimiter": "&",
            "EndOfLine" = 0
            Encoding" = null
            "CultureInfo" : "en-US",
            "Type": 2
            }
        }'

    > C# builder
   
			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForLoadFile("//fileshare//path//load_file.dat)
				.WithDelimiters(d => d
					.WithColumnDelimiters('|')
					.WithQuoteDelimiter('^')
					.WithNewLineDelimiter('#')
					.WithNestedValueDelimiter('&')
					.WithMultiValueDelimiter(';'))
				.WithFirstLineContainingHeaders()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();

    > C#

			DataSourceSettings dataSourceSettings = new DataSourceSettings
			{
				Type = DataSourceType.LoadFile,
				Path = "//fileshare//path//load_file.dat",
				NewLineDelimiter = '#',
				ColumnDelimiter = '|',
				QuoteDelimiter = '^',
				MultiValueDelimiter = ';',
				NestedValueDelimiter = '&',
				Encoding = null,
				CultureInfo = "en-us",
				EndOfLine = DataSourceEndOfLine.Windows,
				FirstLineContainsColumnNames = true,
				StartLine = 0,
			};



4. **Begin Job** 

    > curl

        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/77140fb9-f515-4b65-a2ce-c347492e2905/begin/' 
        -H 'X-CSRF-Header: -' 
        -d ''

5. **End Import Job**  (optional)

    > curl

        curl -X 'POST' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/77140fb9-f515-4b65-a2ce-c347492e2905/end/'
        -H 'X-CSRF-Header: -' 
        -d ''

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
            ....
            ....
        },
        "IsSuccess": true,
        "ErrorMessage": "",
        "ErrorCode": "",
        "ImportJobID": "00000000-0000-0000-0000-000000000000"
        }



## Import Job States

> curl 

    curl -X 'GET' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/ca04baf0-4a1a-4787-94d8-5bba89d2eb0f/details' 
    -H 'X-CSRF-Header: -'

 > response JSON


    {
        "Value": {
            "IsFinished": false,
            "State": "New",
            "ApplicationName": "import demo",
            "Errors": [],
            "CreatedBy": 9,
            "CreatedOn": "2023-01-11T13:47:45.513",
            "LastModifiedBy": 9,
            "LastModifiedOn": "2023-01-11T13:47:45.513"
        },
        "IsSuccess": true,
        "ErrorMessage": "",
        "ErrorCode": "",
        "ImportJobID": "ca04baf0-4a1a-4787-94d8-5bba89d2eb0f"
    }

| value | State                       | Description                                                                                       |
|-------|-----------------------------|---------------------------------------------------------------------------------------------------|
| 10    | New                         | Initial state, job created.                                                                       |
| 13    | Configured                  | Job has been configured and is ready to begin.                                                    |
| 16    | InvalidConfiguration        | Job has been configured but the configuration is invalid.                                         |
| 20    | Idle                        | Job is ready for running but is waiting on new data source or all data source has been processed. |
| 22    | Scheduled                   | Job is ready waiting on queue to begin the process of import.                                     |
| 25    | Inserting                   | Job is executing, import of data source is currently in progress.                                 |
| 26    | PendingCompletion_Scheduled | Job is ended but data source is still waiting on queue to begin the process of import.            |
| 27    | PendingCompletion_Inserting | Job is ended but the import of data source is currently in progress.                              |
| 29    | Paused                      | Job is paused and waiting.                                                                        |
| 30    | Canceled                    | Job canceled.                                                                                     |
| 40    | Failed                      | Job has failed to import data.                                                                    |
| 50    | Completed                   | Job has ended with success..                                                                      |

## Import Data Source States

Data source state can be read get from Data source details response

> curl 

    curl -X 'GET' 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/ca04baf0-4a1a-4787-94d8-5bba89d2eb0f/sources/40ddb007-4330-41cc-b5aa-2ea6961073a5/details' 
    -H 'X-CSRF-Header: -'

 > response JSON

        {
            "Value": {
                "State": "New",
                "DataSourceSettings": {
                    ...
                    ...
                },
                "JobLevelErrors": []
            },
            "IsSuccess": true,
            "ErrorMessage": "",
            "ErrorCode": "",
            "ImportJobID": "ca04baf0-4a1a-4787-94d8-5bba89d2eb0f"
        }


| Value   |   State            |                   Description                                   |
|----|-------------------------|-----------------------------------------------------------------|
| 0  | Unknown                 | Invalid state for a data source                                 |
| 10 | New                     | Initial state, data source was created                          |
| 22 | Scheduled               | Data source is waiting on queue to begin the process of import. |
| 24 | PendingInserting        | Data source has been sent to Worker to begin the import.        |
| 25 | Inserting               | Data source is currently in progress of processing.             |
| 30 | Canceled                | Data source canceled.                                           |
| 40 | Failed                  | Failed to import data from Data source.                         |
| 45 | CompletedWithItemErrors |  Data source processed, import finished with item errors.       |
| 50 | Completed               | Data source processed, import finished.                         |
|    |                         |                                                                 |

# Error Codes

Error handling in import service returns Error Codes and Error Message
 - in every response for failed http request
 - requested by user for all item errors occurred for particular data source e.g.:

 > curl

        curl -X 'GET' 'https://relativity.roadie.so/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/00000000-0000-0000-0000-000000000000/sources/00000000-0000-0000-0000-000000000000/itemerrors?start=0&length=10'
        -H 'accept: application/json' 
        -H 'X-CSRF-Header: -'

    > JSON reponse
                {
        "Value": {
            "DataSourceID": "00000000-0000-0000-0000-000000000000",
            "Errors": [
            {
                "ErrorDetails": [
                {
                    "ColumnIndex": 1,
                    "ErrorCode": "S.LN.INT.0001",
                    "ErrorMessage": "Error message.",
                    "ErrorMessageTemplate": "Template error message.",
                    "ErrorProperties": {
                    "additionalProp1": "string",
                    "additionalProp2": "string",
                    "additionalProp3": "string"
                    }
                }
                ],
                "LineNumber": 1
            }
            ],
            "TotalCount": 1,
            "NumberOfSkippedRecords": 0,
            "NumberOfRecords": 1,
            "HasMoreRecords": false
        },
        "IsSuccess": true,
        "ErrorMessage": "",
        "ErrorCode": "",
        "ImportJobID": "00000000-0000-0000-0000-000000000000"
        }

### Error code structure
===
Error code returned from the Import Service API endpoint has the following structure:

**[Resource].[Action].[ErrorType].[ErrorNumber]**

Examples:

|Error code      |Description                                      |
|----------------|-------------------------------------------------|
|J.CR.VLD.1501   |Cannot create job because validation has failed. |



Error code and error message for particular can be also requested by user:



Resources
---
|Resource code|Description  |
|-------------|-------------|
|J            |Job          |
|C            |Configuration|
|S            |Source       |
|E            |ItemErrors   |
|L            |Libraries    |
|A            |Application  |
|D            |Diagnostic   |
|Q            |Import Queue |
|R            |RDO Conf.    |

Actions
---
|Action   code|Description         |
|-------------|--------------------|
|BEG          |Begin               |
|CR           |Create              |
|CNL          |Cancel              |
|END          |End                 |
|GET          |Get                 |
|GET_COL      |Get columns         |
|GET_CFG      |Get config          |
|GET_DAT      |Get data            |
|GET_DTLS     |Get details         |
|GET_PRG      |Get progress        |
|LN           |Line                |
|PS           |Pause               |
|RD           |Read                |
|RES          |Resume              |
|RUN          |Run                 |
|STAT_CHG     |Handle status change|
|UPD_INF      |Update Info         |
|UPD_PRG      |Update Progress     |
|RSCH         |Reschedule DS import|
|HLT          |App Health check    |
|AGT_HLT      |Agent Helath check  |
|PRV		  |Prv Endpoint access |
|EX			  |Execute             |
|UPD_HB       |Update Heart Beat   |
|CONV         |Convert             |

Error Types
---
|Error type code|Description                         |
|---------------|------------------------------------|
|INT            |Internal service error              |
|EXT            |External dependency error           |
|VLD            |Validation error                    |

Error Numbers
---
Error number has 4 digits. Digits on the first and on the second position has the special meaning.

Meaning of the first digit is the same for all error types.

|Resource code|Description                                    |
|-------------|-----------------------------------------------|
|0XXX         |General error                                  |
|1XXX         |Job related error                              |
|2XXX         |Configuration related error                    |
|3XXX         |Source related error                           |
|4XXX         |ItemErrors related error                       |

Meaning of the second digit differs for each error type.

|Error Type |Resource code|Description                                    |
|-----------|-------------|-----------------------------------------------|
|INT        |X0XX         |General error                                  |
|INT        |X1XX         |Create related error                           |
|INT        |X2XX         |Read related error                             |
|INT        |X3XX         |Update related error                           |
|INT        |X4XX         |Delete related error                           |
|EXT        |X0XX         |Object manager error                           |
|EXT        |X1XX         |SQL error                                      |
|EXT        |X2XX         |Stream error                                   |
|EXT        |X3XX         |Other kepler error                             |
|EXT        |X4XX         |Field manager error                            |
|VLD        |X0XX         |Invalid input data                             |
|VLD        |X5XX         |System state does not allow to execute request |
|VLD        |X6XX         |Data in the system does not exist              |
|VLD        |X7XX         |Data in the system is incorrect                |
|VLD        |X9XX         |Data in the system is corrupted                |

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

 The entire list of import service endpoint: [OpenAPI spec](https://github.com/relativitydev/relativity-import-samples/blob/main/OpenAPI/openapidoc.json)

