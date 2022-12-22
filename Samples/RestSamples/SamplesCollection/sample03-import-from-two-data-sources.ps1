#import
. "$global:rootDir\Helpers\EndpointsClass.ps1"
. "$global:rootDir\Helpers\WriteInformationClass.ps1"

$importId = New-Guid
$source01Id = New-Guid
$source02Id = New-Guid
$workspaceId = 1000000
$loadFile01Path = "C:\DefaultFileRepository\samples\load_file_01.dat"
$loadFile02Path = "C:\DefaultFileRepository\samples\load_file_02.dat"
$global:Endpoints = [Endpoints]::new($workspaceId)
$global:WriteInformation = [WriteInformation]::new()

Context "Sample03 Import from two data sources" {
    Describe "Create job" {
        $uri = $global:Endpoints.importJobCreateUri($importId)

        $body = @{
            applicationName = "Import-service-sample-app"
            correlationID = "Sample-job-0003"
        } | ConvertTo-Json -Depth 10
		
        $response = $global:WebRequest.callPost($uri, $body)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job $importId created" -InformationAction Continue
    }

    Describe "Create document configuration" {
        $uri = $global:Endpoints.documentConfigurationUri($importId)
        $jobConfigurationBody = '{
            "importSettings" :
            {
                "Overlay":null,
                "Native":{
                    "FilePathColumnIndex": "22",
                    "FileNameColumnIndex": "13"
                },
                "Image":null,
                "Production":null,
                "Fields": {
                    "FieldMappings": [
                        {
                            "ColumnIndex": 0,
                            "Field": "Control Number",
                            "ContainsID": false,
                            "ContainsFilePath": false
                        }
                    ]
                },
                "Folder":null
            }
        }'
        $response = $global:WebRequest.callPost($uri, $jobConfigurationBody)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job configuration created" -InformationAction Continue
    }

    Describe "Add Source 01" {
        $uri = $global:Endpoints.importSourceUri($importId, $source01Id)
        $dataSourceConfigurationBody = @{
            dataSourceSettings = @{
                path = $loadFile01Path
                firstLineContainsColumnNames = $true
                startLine = 0
                columnDelimiter = "|"
                quoteDelimiter = "^"
                newLineDelimiter = "#"
                nestedValueDelimiter = "&"
                multiValueDelimiter = "$"
                endOfLine = 0
                encoding = $null
                cultureInfo = "en-us"
                type = 2
            }
        } | ConvertTo-Json -Depth 10
		
        $response = $global:WebRequest.callPost($uri, $dataSourceConfigurationBody)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Source $sourceId added" -InformationAction Continue
    }

    Describe "Add Source 02" {
        $uri = $global:Endpoints.importSourceUri($importId, $source02Id)
        $dataSourceConfigurationBody = @{
            dataSourceSettings = @{
                path = $loadFile02Path
                firstLineContainsColumnNames = $false
                startLine = 0
                columnDelimiter = "|"
                quoteDelimiter = "^"
                newLineDelimiter = "#"
                nestedValueDelimiter = "&"
                multiValueDelimiter = "$"
                endOfLine = 0
                encoding = $null
                cultureInfo = "en-us"
                type = 2
            }
        } | ConvertTo-Json -Depth 10
		
        $response = $global:WebRequest.callPost($uri, $dataSourceConfigurationBody)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Source $sourceId added" -InformationAction Continue
    }

    Describe "Begin job" {
        $uri = $global:Endpoints.importJobBeginUri($importId)
        $beginBody = ""
		
        $response = $global:WebRequest.callPost($uri, $beginBody)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job began" -InformationAction Continue
    }

    Describe "End job" {
        $uri = $global:Endpoints.importJobEndUri($importId)
        $endBody = ""
		
        $response = $global:WebRequest.callPost($uri, $endBody)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "End job called" -InformationAction Continue
    }

    Describe "Wait for import to complete" {
		$uri = $global:Endpoints.importJobDetailsUri($importId)
        $jobDetailsResponse = $global:WebRequest.callGet($uri)
        $isJobFinished = $jobDetailsResponse."Value"."IsFinished"

        [int]$sleepTime = 5

        while($isJobFinished -ne $true)
        {
            Start-Sleep -Seconds $sleepTime
            $jobDetailsResponse = $global:WebRequest.callGet($uri)
            $isJobFinished = $jobDetailsResponse."Value"."IsFinished"
            $state = $jobDetailsResponse."Value"."State"
            Write-Information -MessageData "Current job status: $state" -InformationAction Continue
        }
    }

    Describe "Imported records info" {
        $uri = $global:Endpoints.importSourceDetailsUri($importId, $source01Id)
        $source01DetailsResponse = $global:WebRequest.callGet($uri)
        $state01 = $source01DetailsResponse."Value"."State"
        Write-Information -MessageData "Data source state: $state01" -InformationAction Continue
        $uri = $global:Endpoints.importSourceProgressUri($importId, $source01Id)
        $global:WriteInformation.getDataSourceProgress($uri)

        $uri02 = $global:Endpoints.importSourceDetailsUri($importId, $source02Id)
        $source02DetailsResponse = $global:WebRequest.callGet($uri02)
        $state02 = $source02DetailsResponse."Value"."State"
        Write-Information -MessageData "Data source state: $state02" -InformationAction Continue
        $uri02 = $global:Endpoints.importSourceProgressUri($importId, $source02Id)
        $global:WriteInformation.getDataSourceProgress($uri02)

        #Expected output
        #Example console output for sample files
        #Data source [Id01] state: Completed
        #Data source progress: Total records: 4, Imported records: 4, Records with errors: 0
        #Data source [Id02] state: Completed
        #Data source progress: Total records: 2, Imported records: 2, Records with errors: 0
    }
}