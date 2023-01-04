#import
. "$global:rootDir\Helpers\EndpointsClass.ps1"

$importId = New-Guid
$sourceId = New-Guid
$workspaceId = 1031634
$loadFilePath = "\\emttest\DefaultFileRepository\SampleDataSources\load_file_01.dat" #"C:\DefaultFileRepository\samples\load_file_01.dat"
$global:Endpoints = [Endpoints]::new($workspaceId)

Context "Sample20 Get data source details" {
    Describe "Create job" {
        $uri = $global:Endpoints.importJobCreateUri($importId)

        $body = @{
            applicationName = "Import-service-sample-app"
            correlationID = "Sample-job-0020"
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
                        },
                        {
                            "ColumnIndex": 1,
                            "Field": "Custodian - Single Choice",
                            "ContainsID": false,
                            "ContainsFilePath": false
                        },
                        {
                            "ColumnIndex": 11,
                            "Field": "Email To",
                            "ContainsID": false,
                            "ContainsFilePath": false
                        },
                        {
                            "ColumnIndex": 5,
                            "Field": "Date Sent",
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

    Describe "Add Source" {
        $uri = $global:Endpoints.importSourceUri($importId, $sourceId)
        $dataSourceConfigurationBody = @{
            dataSourceSettings = @{
                path = $loadFilePath
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
		$uri = $global:Endpoints.importSourceDetailsUri($importId, $sourceId)
        $sourceDeatilsResponse = $global:WebRequest.callGet($uri)
        $sourceState = $sourceDeatilsResponse."Value"."State"

        [int]$sleepTime = 5
        $completedStates = @('Completed', 'CompletedWithItemErrors', 'Failed')

        while($completedStates -notcontains $sourceState)
        {
            Start-Sleep -Seconds $sleepTime
            $sourceDeatilsResponse = $global:WebRequest.callGet($uri)
            $sourceState = $sourceDeatilsResponse."Value"."State"
            Write-Information -MessageData "Current data source state: $sourceState" -InformationAction Continue
        }

        Write-Information -MessageData "Data Source finished with state: $sourceState" -InformationAction Continue
    }

    #Expected output
    #Current data source state: Inserting
    #Current data source state: Completed
    #Data Source finished with state: Completed
}