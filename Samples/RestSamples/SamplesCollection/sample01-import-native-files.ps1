#import module
. "$global:rootDir\Helpers\EndpointsClass.ps1"

$importId = New-Guid
$sourceId = New-Guid
$workspaceId = 1000000
$dataSourcePath = "C:\DefaultFileRepository\samples\load_file_01.dat"
$global:Endpoints = [Endpoints]::new($workspaceId, $importId, $sourceId)

Context "Import native files" {
    Describe "Create job" {
        $uri = $global:Endpoints.createImportJobUri

        $body = @{
            applicationName = "Import Sample"
            correlationID = 1234567890
        } | ConvertTo-Json -Depth 10
		
        $response = $global:WebRequest.callPost($uri, $body)
        $global:WebRequest.check($response)
        Write-Information -MessageData "Job $importId created" -InformationAction Continue
    }

    Describe "Create document configuration" {
        $uri = $global:Endpoints.documentConfigurationUri
        $jobConfigurationBody = '{
            "importSettings" :
            {
                "Overlay": {
                            "Mode" : 1,
                            "KeyField" : "Control Number"
                        },
                "Native":{
                    "FilePathColumnIndex": "22",
                    "FileNameColumnIndex": "13"
                },
                "Image":null,
                "Production":null,
                "Fields": {
                    "FieldMappings": [
                        {
                            "ColumnIndex": "0",
                            "Field": "Control Number",
                            "ContainsID": false,
                            "ContainsFilePath": "false"
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
        $global:WebRequest.check($response)
        Write-Information -MessageData "Job configuration created" -InformationAction Continue
    }

    Describe "Add Source" {
        $uri = $global:Endpoints.importSourcesUri
        $dataSourceConfigurationBody = @{
            dataSourceSettings = @{
                path = $dataSourcePath
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
        $global:WebRequest.check($response)
        Write-Information -MessageData "Source $sourceId added" -InformationAction Continue
    }

    Describe "Begin job" {
        $uri = $global:Endpoints.beginImportJobUri
        $beginBody = ""
		
        $response = $global:WebRequest.callPost($uri, $beginBody)
        $global:WebRequest.check($response)
        Write-Information -MessageData "Job began" -InformationAction Continue
    }

    Describe "End job" {
        $uri = $global:Endpoints.endImportJobUri
        $endBody = ""
		
        $response = $global:WebRequest.callPost($uri, $endBody)
        $global:WebRequest.check($response)
        Write-Information -MessageData "End job called" -InformationAction Continue
    }

    Describe "Wait for import to complete" {
		$uri = $global:Endpoints.importJobDetailsUri
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
		$uri = $global:Endpoints.importSourceDetailsUri
        $sourceDetailsResponse = $global:WebRequest.callGet($uri)
        $state = $sourceDetailsResponse."Value"."State"

        $uri = $global:Endpoints.importSourceProgressUri
        $sourceProgresssResponse = $global:WebRequest.callGet($uri)
        $totalRecords = $sourceProgresssResponse."Value"."TotalRecords"
        $impoortedRecords = $sourceProgresssResponse."Value"."ImportedRecords"
        $erroredRecords = $sourceProgresssResponse."Value"."ErroredRecords"

        Write-Information -MessageData "Data source state: $state"  -InformationAction Continue
        Write-Information -MessageData "Import progress: Total records: $totalRecords, Imported records: $impoortedRecords, Records with errors: $erroredRecords"  -InformationAction Continue

        #Expected output
        #Data source state: Completed
        #Import progress: Total records: 4, Imported records: 4, Records with errors: 0
    }
}