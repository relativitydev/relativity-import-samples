#import
. "$global:rootDir\Helpers\EndpointsClass.ps1"
. "$global:rootDir\Helpers\WriteInformationClass.ps1"

$workspaceId = 1000000
$opticonFilePath =  "C:\DefaultFileRepository\samples\opticon_01.opt"

$importId = New-Guid
$sourceId = New-Guid
$global:Endpoints = [Endpoints]::new($workspaceId)
$global:WriteInformation = [WriteInformation]::new()

Context "Sample10 Import images in append overlay mode" {
    Describe "Create job" {
        $uri = $global:Endpoints.importJobCreateUri($importId)

        $body = @{
            applicationName = "Import-service-sample-app"
            correlationID = "Sample-job-image-0010"
        } | ConvertTo-Json -Depth 10
		
        $response = $global:WebRequest.callPost($uri, $body)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job $importId created" -InformationAction Continue
    }

    Describe "Create document configuration" {
        $uri = $global:Endpoints.documentConfigurationUri($importId)
        $jobConfigurationBody = '{
            "importSettings": {
                "Overlay": {
                    "Mode": 3
                },
                "Native": null,
                "Image": {
                    "PageNumbering": 1,
                    "ProductionID": null,
                    "LoadExtractedText": false,
                    "FileType": 0
                },
                "Fields": null,
                "Folder": null
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
                Path = $opticonFilePath
                EndOfLine = 0
                StartLine = 0
                Type = 1
            }
        } | ConvertTo-Json  -Depth 10
		
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
        $uri = $global:Endpoints.importSourceDetailsUri($importId, $sourceId)
        $sourceDetailsResponse = $global:WebRequest.callGet($uri)
        $state = $sourceDetailsResponse."Value"."State"
        Write-Information -MessageData "Data source state: $state" -InformationAction Continue
        $uri = $global:Endpoints.importSourceProgressUri($importId, $sourceId)
        $global:WriteInformation.getDataSourceProgress($uri)

        #Expected output
        #Data source state: Completed
        #Data source progress: Total records: 5, Imported records: 5, Records with errors: 0
    }
}