# relativity-import

### Table of Contents
### **[Introduction](#introduction)**
**[Prerequisites](#prerequisites)**<br>
**[Glossary](#glossary)**<br>
### **[Getting Started](#getting-started)**
**[NuGet Libraries](#importservicesdk)**<br>
**[Authorization](#authorization)**<br>
**[Permissions](#permissions)**<br>
**[Builders](#builders)**<br>
**[General Import flow description ](#general-import-flow-description)**<br>
**[Example Import flows](#example-of-simple-import-documents-flow)**<br>
**[API Documentation](#rest-api)**<br>
**[API Response](#api-response)**<br>
**[ImportJob & DataSource States](#rest-api)**<br>
**[Error Codes](#error-codes)**<br>
### **[Code Samples](#samples)**
**[.NET 7 Console Application - How-to](#net-7-code-samples---how-to)**<br>
**[.NET Framework & Kepler Console Application - How-to](#keplerclient-code-samples---how-to)**<br>
**[Powershell scripts - How-to](#powershell-script-samples---how-to)**<br>



# Introduction
The ***Relativity Import Service API***  is a Kepler service that provides functionality for importing large numbers of documents, images, and Relativity Dynamic Objects (RDOs) into a Relativity workspace. 
The import process operates on structured data sets that are described by load file and located in a place accessible for workspace.  
The main principle of operation is based on creating managed importing job with a list of data sets (intended for import) assigned to it.
<br>

Thanks to RESTful API you are able to easily create import job, configure it and run it. 
Dataset (containing structured data) that you want to import can be then added as a source to the job. The system will take care of it in the background by adding this source to the queue, scheduling and finally starting the import data to destination workspace, and if necessary, resuming the import process. All that remains for the user is to monitor the status of import and current progress - all using  provided API.

Job and data sources configurations allow you to flexibly adjust the import to your needs. In addition, the adopted error handling helps you to identify the source of potential problems. 

 NOTE: Import Service (*Import*) is delivered as a RAP application installed in Relativity One.

# Prerequisites

1. The following Relativity applications must be installed:



    | application name     | application Guid                      | where installed     |
    |----------------------|---------------------------------------|---------------------|
    |*Import*              | 21F65FDC-3016-4F2B-9698-DE151A6186A2  |  workspace          |
    |*DataTransfer.Legacy* | 9f9d45ff-5dcd-462d-996d-b9033ea8cfce  |  instance           |


2. Appropriate user [permissions](#permissions) need to be set. 


3. Data set - load files, source files (native documents, images, text files) - need to be placed in the destination fileshare location accessible to workspace. 


4. The following packages installed in client application:
   - [Import.Service.SDK](#importservicesdk) 
   - [Relativity.Kepler.Client.SDK](https://www.nuget.org/packages/Relativity.Kepler.Client.SDK)

   *NOTE*: Required only when Kepler .NET client is used.
***
## Glossary

**Data importing** - Functionality that makes that structured data set are uploading into destination workspace.

**Dataset** - Structured data containing metadata, native documents, images, text files described by load file or opticon file.
Such a dataset can be pointed during data source configuration and MUST be located in accessible place for workspace. 

**ImportJob** - It is the main object in import service taking part in import flow. It represents single import entity described by its configuration which decides about import behavior e.g. import type, overlay mode, fields mapping, etc.  
In addition, ImportJob object holds the information about its current state and importing progress.
Import jobs aggregates dataSources - single import job can consists of many sources.

**DataSource**  - It is an object that corresponds to single set of data to be imported. Each data source has own configuration that indicates the physical location of data set (load file). Data set configuration affects also how data in load file are read.
In addition, data source stores the information about current state and import progress of particular source.

**Kepler service** - API service created but using the Relativity Kepler framework. This framework provides you with the ability to build custom REST Endpoints via a .NET interface. Additionally, the Kepler framework includes a client proxy that you can use when interacting with the services through .NET. [See more information](https://platform.relativity.com/RelativityOne/index.htm#Kepler_framework/Kepler_framework.htm#Client-s)

---
# Getting Started ##
Import Service is built as a standard Relativity Kepler Service. It provides sets of endpoints that must be called sequentially in order to execute import.
The following sections outline how to make calls to import service.
    
> HTTP clients 

 You can make calls to a import service using any standard REST or HTTP client, because all APIs (Keplers APIs) are exposed over the HTTP protocol. You need to set the required X-CSRF-Header. [more details](https://platform.relativity.com/RelativityOne/index.htm#Kepler_framework/Kepler_framework.htm#Client-s) 

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-CSRF-Header", "-");

        createImportJobUri = $"{host}/Relativity.REST/api/import.service/v1....
        
        var response = await httpClient.PostAsJsonAsync(createImportJobUri, payload);

In case of using .NET client the [Import.Service.SDK.Models](#importservicesdkmodels) package containing contract models would be used.

  Please look at dedicated [code samples](#samples) for .NET 7 or for PowerShell scripts.

> Kepler .NET client

You can access Kepler service from any .NET language using the client library provided as part of the Kepler framework. It exposes a factory class that you can use to create the client proxy by passing URIs to import services and credentials. Then use .NET proxy to interact with a import service as a set of .NET objects. When you call a member method, the proxy makes a corresponding HTTP request to the respective service endpoint. [more details](https://platform.relativity.com/RelativityOne/index.htm#Kepler_framework/Kepler_framework.htm#Client-s) 

Kepler contract for import service are exposed in [Import.Service.SDK](importservicesdk) package.


        using (Relativity.Import.V1.Services.IImportJobController importJobController =_serviceFactory.CreateProxy<Relativity.Import.V1.Services.IImportJobController>())
        {
            // Create import job.
            Response response = await importJobController.CreateAsync(
                importJobID: importId,
                workspaceID: workspaceId,
                applicationName: "Import-service-sample-app",
                correlationID: "Sample-job-0001");
            }
        }

  Please look at dedicated [code samples](#samples) for .NET 4.6.2 with Kepler.

---
## Import.Service.SDK ###

Import.Service.SDK is a .NET library that contains kepler interfaces for import service.
It provides and simplifies executing import in client application.
Import.Service.SDK targets .NET Framework 4.6.2

**NOTE: Use this package when your application USE keplers.**


[![Version](https://img.shields.io/nuget/v/Import.Service.SDK.svg?color=royalblue)](https://www.nuget.org/packages/Import.Service.SDK)
[![Downloads](https://img.shields.io/nuget/dt/Import.Service.SDK?color=green)](https://www.nuget.org/packages/Import.Service.SDK)
<br>  

### **Installing via NuGet** 

        Install-Package Import.Service.SDK 

## Import.Service.SDK.Models ###
Import.Service.SDK.Models is a .NET library that contains contract models for API and [builders](#builders) which help user to prepare payloads in correct and consistent way.
Import.Service.SDK.Models targets .NET Standard 2.0. The NuGet package also includes direct targets for .NET Framework 4.6.2.
<br/>  
**NOTE:**
This package is automatically installed as dependency when using Import.Service.SDK.

**NOTE:** You can install this package directly when your application does not use keplers.
<br/> 


[![Version](https://img.shields.io/nuget/v/Import.Service.SDK.Models.svg?color=royalblue)](https://www.nuget.org/packages/Import.Service.SDK.Models)
[![Downloads](https://img.shields.io/nuget/dt/Import.Service.SDK.Models?color=green)](https://www.nuget.org/packages/Import.Service.SDK.Models)
### **Installing via NuGet** 
        Install-Package Import.Service.SDK.Models

---
## Authorization
> HTTP clients 

Import Service API conforms to the same authentication rules as other Relativity REST APIs. 

The more details can be found under the following link:
[REST_API_authentication](https://platform.relativity.com/RelativityOne/Content/REST_API/REST_API_authentication.htm)

> Kepler .NET client

The Kepler framework uses a proxy to handle client requests. The more details can be found under the following link:
[Proxies_and_authentication](https://platform.relativity.com/RelativityOne/Content/Kepler_framework/Proxies_and_authentication.htm#Service)

---
## Permissions
The following Relativity permissions are required to use import features provided in Import Service API.

| Object Security section| Permission |
| :---- | :----: |
| •	Document |	View, Add, Edit |
| •	Relativity Import Job: |	View, Add, Edit |
| •	Relativity Import Data Source |	View, Add, Edit |

| Tab Visibility|
| :---- |
| •	Documents |

| Admin Operation|
| :---- |
| •	Allow Import |
---
<br>  

## Builders

Builders provided in Import.Service.SDK.Models package help to create settings for import job and data source in correct and consistent way. It is highly recommended to prepare these objects in such a way in .NET application. They are implemented in fluent api pattern so it is very easy to use them. Moreover, using them in client application will avoid the risk of incorrect and inconsistent configuration
which may lead to errors during import process.

*ImportDocumentsSettingsBuilder* - builds ImportDocumentsSettings used for import job configuration (documents import).

*ImportRdoSettingsBuilder* - builds ImportRdoSettings used for import job configuration (rdos import).

*DataSourceSettingsBuilder* - builds DataSourceSettings used for data source configuration.

> C#

    // Example of using ImportDocumentSettingsBuilder to create ImportDocumentSettings.

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


> C#

    // Example of using DataSourceSettingsBuilder to create DataSourceSettings.

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


NOTE: Please review the samples to find more about builders.
***
## General Import flow description
The general flow includes several steps consisted in sending appropriate HTTP requests.

1. **Create Import Job** 

   Creates import job entity in particular workspace. Job is defined by its unique Id generated by user and provided in the request which is used in the next steps.

2. **Configure Import Job**

    Configures existing import job by defining sets of significant parameters including import type, its mode, fields mapping. This step covers two configuration options: 
    - Documents Configuration 

    - Rdos Configuration.
  
3. **Add one or multiple DataSources**

    Creating data source entity or entities for particular import job. It represents the configuration that corresponds to dataset being imported. Data source is identified by its unique Id generated by user and provided in the request. Data source configuration includes path to “load file” and other significant parameters telling how data in load file will be read and interpreted by system.

    Many data sources can be added to the same import job. Data sources can be added both before job is started and after.


4. **Begin Job**  
  Starts Import Job which enables the process that schedules importing data to workspace based on the configuration assigned in previous steps.
  Started job does not mean that data are instantly imported. However DataSources are added to the queue and scheduled by background mechanism.
  The import Job state or data source state shows the current stage. 
 
5. **Add one or multiple DataSources to running job**
  User can add additional sources to running importJob.

6. **End Import Job** 
  Ends import job that was already started. It is optional step but it is highly recommended in case when no more data source is plan to be added for particular job. All data sources added to the job before the end request was sent will be imported.

## Example of Simple Import Documents Flow
1.  **Create Import Job** 
        
    > curl
    
         curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/e694ad62-198d-4ecb-936d-1862ddfa4235'
        -H 'X-CSRF-Header: -' 
        -d '{
        "applicationName": "simpleImportDocuments",
        "correlationID": "c0r31ati0n_ID"
        }'
<br>

2. **Create Import Job Configuration** 

    > curl

        curl -X POST \'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/e694ad62-198d-4ecb-936d-1862ddfa4235/documents-configurations/' 
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

        curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/e694ad62-198d-4ecb-936d-1862ddfa4235/sources/0cb922a2-8df4-42fd-9429-c241410a0d1e'
        -H 'X-CSRF-Header: -' \
        -H 'Content-Type: application/json' 
        -d "$dataSourceSettingsJson"
        }'

    Data source configuration payload example:

    > JSON

        {
        "dataSourceSettings": {
            "Path": "C:\\DefaultFileRepository\\samples\\load_file.dat",
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

    > C# builders
   
			DataSourceSettings dataSourceSettings = DataSourceSettingsBuilder.Create()
				.ForLoadFile("C:\\DefaultFileRepository\\samples\\load_file.dat")
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
				Path = "C:\\DefaultFileRepository\\samples\\load_file.dat",
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

        curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/e694ad62-198d-4ecb-936d-1862ddfa4235/begin/' 
        -H 'X-CSRF-Header: -' 
        -d ''

<br>

5. **End Import Job** 

    > curl

        curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/e694ad62-198d-4ecb-936d-1862ddfa4235/end/'
        -H 'X-CSRF-Header: -' 
        -d ''



<br><br>

## Example of Simple Import Images Flow
1. **Create Import Job** 
    > curl

        curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/'
        -H 'X-CSRF-Header: -' 
        -d '{
        "applicationName": "simpleImportImages",
        "correlationID": "img0r22ati0n_ID"
        }'

2. **Create Import Job Configuration**
    > curl

        curl -X POST \'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/documents-configurations/' 
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

            curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/sources/0cb922a2-8df4-42fd-9429-c241410a0002'
            -H 'X-CSRF-Header: -' \
            -H 'Content-Type: application/json' 
            -d "$dataSourceSettingsJson"
            }'

    > JSON
    
        {
        "dataSourceSettings": {
            "Path": "C:\\DefaultFileRepository\\samples\\opticon_file.opt",
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
				.ForOpticonFile("C:\\DefaultFileRepository\\samples\\opticon_file.opt")
				.WithDefaultDelimitersForOpticonFile()
				.WithEndOfLineForWindows()
				.WithStartFromBeginning()
				.WithDefaultEncoding()
				.WithDefaultCultureInfo();

    > C#

			DataSourceSettings dataSourceSettings = new DataSourceSettings
			{
				Type = DataSourceType.Opticon,
				Path = "C:\\DefaultFileRepository\\samples\\opticon_file.opt",
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

        curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/begin/' 
        -H 'X-CSRF-Header: -' 
        -d ''

5. **End Import Job** 
    > curl

        curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/end/'
        -H 'X-CSRF-Header: -' 
        -d ''

<br><br>

## Example of Simple Import Relativity Dynamic Objects (RDO) Flow

1. **Create Import Job** 
    > curl
    
        curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/77140fb9-f515-4b65-a2ce-c347492e2905/'
        -H 'X-CSRF-Header: -' 
        -d '{
        "applicationName": "simpleImportRdo",
        "correlationID": "rdor31ati0n_ID"
        }'

2. **Create Import Job Configuration**

    > curl

        curl -X POST \'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/77140fb9-f515-4b65-a2ce-c347492e2905/rdo-configurations/' 
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


3. **Add DataSource** 
    > curl

        curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/4c4215bf-d8a3-48d4-a3e0-3a40428415e7/sources/0cb922a2-8df4-42fd-9429-c241410a0002'
        -H 'X-CSRF-Header: -' \
        -H 'Content-Type: application/json' 
        -d "$dataSourceSettingsJson"
        }'

    Data source configuration payload example:

    > JSON

        {
        "dataSourceSettings": {
            "Path": "C:\\DefaultFileRepository\\samples\\load_file.dat",
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
				.ForLoadFile("C:\\DefaultFileRepository\\samples\\load_file.dat)
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
				Path = "C:\\DefaultFileRepository\\samples\\load_file.dat",
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

        curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/77140fb9-f515-4b65-a2ce-c347492e2905/begin/' 
        -H 'X-CSRF-Header: -' 
        -d ''

5. **End Import Job** 

    > curl

        curl -X POST 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/77140fb9-f515-4b65-a2ce-c347492e2905/end/'
        -H 'X-CSRF-Header: -' 
        -d ''
---

### REST API

 Review the open API spec for import service: [OpenAPI spec](https://github.com/relativitydev/relativity-import-samples/blob/main/OpenAPI/openapidoc.json)


## API Response

Each HTTP response to POST request has unified schema:

        {
        "IsSuccess": true,
        "ErrorMessage": "",
        "ErrorCode": "",
        "ImportJobID": "00000000-0000-0000-0000-000000000000"
        }

Each HTTP response to GET requests has unified schema:

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

---

## Import Job States

Import job state can be read from  GET Import Job details response

> curl 

    curl -X GET 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/ca04baf0-4a1a-4787-94d8-5bba89d2eb0f/details' 
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
| 50    | Completed                   | Job has ended with success.                                                                      |

## Import Data Source States

Data source state can be read from GET Data source details response

> curl 

    curl -X GET 'https://relativity-host/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/ca04baf0-4a1a-4787-94d8-5bba89d2eb0f/sources/40ddb007-4330-41cc-b5aa-2ea6961073a5/details' 
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
| 0  | Unknown                 | Invalid state for a data source.                                |
| 10 | New                     | Initial state, data source was created.                         |
| 22 | Scheduled               | Data source is waiting on queue to begin the process of import. |
| 24 | PendingInserting        | Data source has been sent to Worker to begin the import.        |
| 25 | Inserting               | Data source is currently in progress of processing.             |
| 30 | Canceled                | Data source canceled.                                           |
| 40 | Failed                  | Failed to import data from Data source.                         |
| 45 | CompletedWithItemErrors |  Data source processed, import finished with item errors.       |
| 50 | Completed               | Data source processed, import finished.                         |
|    |                         |                                                                 |

# Error Codes

Error handling in Import Service returns Error Codes and Error Messages:
 - in every response for failed HTTP request
 - when requested by user for all item errors that occurred during importing particular data source e.g.:

 > curl

        curl -X GET 'https://relativity.roadie.so/Relativity.REST/api/import-service/v1/workspaces/10000/import-jobs/00000000-0000-0000-0000-000000000000/sources/00000000-0000-0000-0000-000000000000/itemerrors?start=0&length=10'
        -H 'accept: application/json' 
        -H 'X-CSRF-Header: -'


  > JSON response

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

## Error code structure

Error code returned from the Import Service API endpoint has the following structure:

**[Resource].[Action].[ErrorType].[ErrorNumber]**

Examples:

|Error code      |Description                                      |
|----------------|-------------------------------------------------|
|J.CR.VLD.1501   |Cannot create job because validation has failed. |




Resources
---
|Resource code|Description  |
|-------------|-------------|
|J            |Job          |
|C            |Document Configuration|
|S            |Source       |
|E            |ItemErrors   |
|R            |RDO Configuration    |

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
|INT        |X[0-9]XX     |Service errors                                  |
|EXT        |X[0-9]XX     |Runtime errors                                  |
|VLD        |X0XX         |Invalid input data                             |
|VLD        |X5XX         |System state does not allow to execute request |
|VLD        |X6XX         |Data in the system does not exist              |
|VLD        |X7XX         |Data in the system is incorrect                |
|VLD        |X9XX         |Data in the system is corrupted                |


<br><br>

---
# Samples

## Samples types and structure
There are three types of sample application that demonstrate the use of Import Service API features.
- .Net7ConsoleClient - .NET console application  (.NET 7, C#).
- KeplerClientConsole - .NET console application (.NET Framework 4.6.2, Kepler client, C#).
- REST client -  Powershell scripts.

Examples structure:
- Each code example for particular import flow is contained in separate file (e.g. [Sample04_AddDataSourceToRunningJob.cs](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample04_AddDataSourceToRunningJob.cs)).
- Each sample is numbered. Number of sample is included in file and class name.
- The individual samples for each application are consistent. For instance Sample_08 in Net7ConsoleClient presents the same import flow as in KeplerClientConsole and in PS scripts.
- Sample code contains accurate comments describing the flow. 
- Expected console result of demonstrated sample flow is included at the end of the file. 
- Repository with samples contains also structured data set used in all examples - load files, opticon files, folders structure with native files, images, text files.

List of samples:
| Sample name | .Net with Kepler| .NET | PowerShell |
| :---- | :----: | :----: | :----: |
 | Sample01_ImportNativeFiles | [Sample01](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample01_ImportNativeFiles.cs) | [Sample01](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample01_ImportNativeFiles.cs) | [Sample01](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample01-import-native-files.ps1) | 
 | Sample02_ImportDocumentsInOverlayMode | [Sample02](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample02_ImportDocumentsInOverlayMode.cs) | [Sample02](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample02_ImportDocumentsInOverlayMode.cs) | [Sample02](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample02-import-documents-in-overlay-mode.ps1) | 
| Sample03_ImportFromTwoDataSources | [Sample03](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample03_ImportFromTwoDataSources.cs) | [Sample03](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample03_ImportFromTwoDataSources.cs) | [Sample03](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample03-import-from-two-data-sources.ps1) | 
| Sample04_AddDataSourceToRunningJob|  [Sample04](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample04_AddDataSourceToRunningJob.cs) | [Sample04](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample04_AddDataSourceToRunningJob.cs) | [Sample04](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample04-add-data-source-to-running-job.ps1) | 
| Sample05_ImportDocumentsWithExtractedText | [Sample05](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample05_ImportDocumentsWithExtractedText.cs) | [Sample05](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample05_ImportDocumentsWithExtractedText.cs) | [Sample05](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample05-import-documents-with-extracted-text.ps1) | 
| Sample06_ImportDocumentsToSelectedFolder | [Sample06](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample06_ImportDocumentsToSelectedFolder.cs) | [Sample06](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample06_ImportDocumentsToSelectedFolder.cs) | [Sample06](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample06-import-documents-to-selected-folder.ps1) | 
| Sample07_DirectImportSettingsForDocuments.cs | [Sample07](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample07_DirectImportSettingsForDocuments.cs) | [Sample07](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample07_DirectImportSettingsForDocuments.cs) | [Sample07](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample07-direct-import-settings-for-documents.ps1) | 
| Sample08_ImportImages | [Sample08](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample08_ImportImages.cs) | [Sample08](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample08_ImportImages.cs) | [Sample08](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample08-import-images.ps1) | 
 | Sample09_ImportProductionFiles | [Sample09](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample09_ImportProductionFiles.cs) | [Sample09](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample09_ImportProductionFiles.cs) | [Sample09](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample09-import-production-files.ps1) | 
 | Sample10_ImportImagesInAppendOverlayMode |[Sample10](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample10_ImportImagesInAppendOverlayMode.cs) | [Sample10](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample10_ImportImagesInAppendOverlayMode.cs) | [Sample10](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample10-import-images-in-append-overlay-mode.ps1) | 
 | Sample11_DirectImportSettingsForImages| [Sample11](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample11_DirectImportSettingsForImages.cs) | [Sample11](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample11_DirectImportSettingsForImages.cs) | [Sample11](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample11-direct-import-settings-for-images.ps1) | 
 | Sample12_ImportRelativityDynamicObject | [Sample12](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample12_ImportRelativityDynamicObject.cs) | [Sample12](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample12_ImportRelativityDynamicObject.cs) | [Sample12](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample12-import-relativity-dynamic-object.ps1) | 
 | Sample13_ImportRdoWithParent | [Sample13](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample13_ImportRdoWithParent.cs) | [Sample13](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample13_ImportRdoWithParent.cs) | [Sample13](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample13-import-rdo-with-parent.ps1) | 
 | Sample14_DirectImportSettingsForRdo | [Sample14](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample14_DirectImportSettingsForRdo.cs) | [Sample14](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample14_DirectImportSettingsForRdo.cs) | [Sample14](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample14-direct-import-settings-for-rdo.ps1) | 
 | Sample15_ReadImportRdoSettings|  [Sample15](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample15_ReadImportRdoSettings.cs) | [Sample15](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample15_ReadImportRdoSettings.cs) | [Sample15](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample15-read-import-rdo-settings.ps1) | 
 | Sample16_ReadImportDocumentSettings | [Sample16](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample16_ReadImportDocumentSettings.cs) | [Sample16](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample16_ReadImportDocumentSettings.cs) | [Sample16](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample16-read-import-document-settings.ps1) | 
 | Sample17_GetImportJobs | [Sample17](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample17_GetImportJobs.cs) | [Sample17](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample17_GetImportJobs.cs) | [Sample17](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample17-read-import-jobs.ps1) | 
 | Sample18_GetDataSource | [Sample18](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample18_GetDataSource.cs) | [Sample18](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample18_GetDataSource.cs) | [Sample18](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample18-get-data-source.ps1) | 
 | Sample19_GetImportJobDetailsAndProgress | [Sample19](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample19_GetImportJobDetailsAndProgress.cs) | [Sample19](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample19_GetImportJobDetailsAndProgress.cs) | [Sample19](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample19-get-import-job-details-and-progress.ps1) | 
 | Sample20_GetDataSourceDetailsAndProgress | [Sample20](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample20_GetDataSourceDetailsAndProgress.cs) | [Sample20](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample20_GetDataSourceDetailsAndProgress.cs) | [Sample20](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample20-get-data-source-details-and-progress.ps1) | 
 | Sample21_CancelStartedJob | [Sample21](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample21_CancelStartedJob.cs) | [Sample21](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample21_CancelStartedJob.cs) | [Sample21_](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample21-cancel-started-job.ps1) | 
 | Sample22_ReadResponse | [Sample22](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample22_ReadResponse.cs) | [Sample22](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample22_ReadResponse.cs) | [Sample22](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample22-read-response.ps1) | 
 | Sample23_GetDataSourceErrors | [Sample23](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/SampleCollection/Sample23_GetDataSourceErrors.cs) | [Sample23](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/SamplesCollection/Sample23_GetDataSourceErrors.cs) | [Sample23](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/RestSamples/SamplesCollection/sample23-get-data-source-errors.ps1) | 

## .NET 7 Code Samples - How to

 
 To run a sample code:
 
 - Copy the content of [sample dataset](https://github.com/relativitydev/relativity-import-samples/tree/main/SampleDataSources) to your Relativity fileshare.
 - Uncomment line with sample invocation you want to run in Main method.  

            // await sampleCollection.Sample08_ImportImages();
            // await sampleCollection.Sample09_ImportProductionFiles();

            await sampleCollection.Sample10_ImportImagesInAppendOverlayMode();

 - Set the proper credentials and URI of your Relativity instance in [RelativityUserSettings](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/.Net7ClientConsole/Helpers/RelativityUserSettings.cs) helper class.

        public class RelativityUserSettings
        {
            public const string BaseAddress = "https://host/Relativity.REST/";

            public const string UserName = "sample.user@test.com";

            public const string Password = "password!";
        }

 - Update workspaceId const to the proper value equals Id of the workspace where you intend to import data. It is required in each sample. 

 
        // destination workspace artifact Id.
        const int workspaceId = 1000000;

 - Update other Ids related to your workspace - productionSetsArtifactId , rootFolderId,rdoArtifactTypeID. They are required only by specific samples.
 - Update const which defines the path to the load file (e.g. const string  loadFile01Path) according to the location where you copied sample data.

 
			// Path to the file in opticon format used in data source settings.
			const string opticonFilePath = "X:\\DefaultFileRepository\\samples\\opticon_01.opt";

 - Run application 



## KeplerClient Code Samples - How to
 
 To run a sample code:
 
 - Copy the content of [sample dataset](https://github.com/relativitydev/relativity-import-samples/tree/main/SampleDataSources) to your Relativity fileshare.
 - Uncomment line with sample invocation you want to run in Main method.
 - Set the proper credentials and host address of your Relativity instance in [RelativityUserSettings](https://github.com/relativitydev/relativity-import-samples/blob/main/Samples/KeplerClientConsole/ImportSampleHelpers/RelativityUserSettings.cs) helper class.


 	    public static class RelativityUserSettings
	    {
		public const string HostAddress = "https://host-address";

		public const string UserName = "sample.user@test.com";

		public const string Password = "password!";
	    }

 - Update workspaceId const to the proper value equal Id of the workspace where you intend to import data. It is required in each sample. 
 - Update other Ids related to your workspace - productionSetsArtifactId , rootFolderId,rdoArtifactTypeID. They are required only by specific samples.
 - Update const which defines the path to the load file (e.g. const string  loadFile01Path) according to the location where you copied sample data.
 - Run application 

## Powershell Script Samples - How to
 
 To run a sample code:
 
 - Install Powershell "Pester" Module  (ver. >= 5.3.3)
 
        Find-Module -Name "Pester" | Install-Module -Force;
 - Copy the content of [sample dataset](https://github.com/relativitydev/relativity-import-samples/tree/main/SampleDataSources) to your Relativity fileshare.

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

 - Update other Ids related to your workspace - productionSetsArtifactId , rootFolderId,rdoArtifactTypeID. They are required only by specific samples.


 - Invoke run-sample-import.ps1


