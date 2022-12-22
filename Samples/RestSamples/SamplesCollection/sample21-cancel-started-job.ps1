#import
. "$global:rootDir\Helpers\EndpointsClass.ps1"
. "$global:rootDir\Helpers\WriteInformationClass.ps1"

$workspaceId = 1000000
$importId = New-Guid
$sourcesCount = 7
$loadFilePath = "C:\DefaultFileRepository\samples\load_file_07_"
$global:Endpoints = [Endpoints]::new($workspaceId)
$global:WriteInformation = [WriteInformation]::new()

Context "Sample21 Cancel stared Job" {
    Describe "Create job" {
        $uri = $global:Endpoints.importJobCreateUri($importId)

        $body = @{
            applicationName = "Import-service-sample-app"
            correlationID = "Sample-job-0021"
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
                "Overlay":{
                    "Mode": 3
                },
                "Native":{
                    "FilePathColumnIndex": "11",
                    "FileNameColumnIndex": "13"
                },
                "Image":null,
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

    Describe "Add n data sources to the existing job." {
        $loadFilePath = $loadFilePath.replace('\','\\')

        for ($i = 0; $i -lt $sourcesCount; $i++) {
            $sourceId = New-Guid
            $uri = $global:Endpoints.importSourceUri($importId, $sourceId)            

            $dataSourceConfigurationBody = @{
                dataSourceSettings = @{
                    path = $loadFilePath + $i + ".dat"
                    firstLineContainsColumnNames = $true
                    startLine = 0
                    columnDelimiter = "\u0014"
                    quoteDelimiter = "\u00fe"
                    newLineDelimiter = "\u00ae"
                    nestedValueDelimiter = "\u005c"
                    multiValueDelimiter = "\u003b"
                    endOfLine = 0
                    encoding = $null
                    cultureInfo = "en-us"
                    type = 2
                }
            } | ConvertTo-Json -Depth 10 | Foreach {[System.Text.RegularExpressions.Regex]::Unescape($_)}
            
            $response = $global:WebRequest.callPost($uri, $dataSourceConfigurationBody)
            $global:WebRequest.checkIfSuccess($response)
            Write-Information -MessageData "Source $sourceId added" -InformationAction Continue
        }
        Write-Information -MessageData "$sourcesCount sources added" -InformationAction Continue
    }

    Describe "Begin job" {
        $uri = $global:Endpoints.importJobBeginUri($importId)
        $beginBody = ""
		
        $response = $global:WebRequest.callPost($uri, $beginBody)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job began" -InformationAction Continue
    }

    Describe "Cancel job" {
		$uri = $global:Endpoints.importJobDetailsUri($importId)
        $jobDetailsResponse = $global:WebRequest.callGet($uri)
        $state = $jobDetailsResponse."Value"."State"
        Write-Information -MessageData "Current job status: $state" -InformationAction Continue

        $cancelUri = $global:Endpoints.importJobCancelUri($importId)
        $cancelResponse = $global:WebRequest.callPost($cancelUri, "")

        $jobDetailsResponse = $global:WebRequest.callGet($uri)
        $state = $jobDetailsResponse."Value"."State"
        Write-Information -MessageData "Current job status: $state" -InformationAction Continue
    }

        <# Expected output
            Current job status: Inserting
            POST request url https://sample-hosts/Relativity.REST/api/import-service/v1/workspaces/1000000/import-jobs/$importId/cancel
            Body
            Current job status: Canceled
        #>
}