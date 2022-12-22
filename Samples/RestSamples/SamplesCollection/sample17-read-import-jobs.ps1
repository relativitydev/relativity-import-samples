#import
. "$global:rootDir\Helpers\EndpointsClass.ps1"

$workspaceId = 1000000
$importJobsCount = 10
$pageSize = 7
$global:Endpoints = [Endpoints]::new($workspaceId)

$jobConfigurationBody = '{
    "importSettings" :
    {
        "Overlay":null,
        "Native":{
            "FilePathColumnIndex": "22",
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
                },
                {
                    "ColumnIndex": 11,
                    "Field": "Email To",
                    "ContainsID": false,
                    "ContainsFilePath": false
                }
            ]
        },
        "Folder":null
    }
}'

Context "Sample17 Read import jobs for workspace" {
    Describe "Create and configure n import jobs" {
        for ($i = 0; $i -lt $importJobsCount; $i++) {
            $importId = New-Guid
            $uri = $global:Endpoints.importJobCreateUri($importId)

            $body = @{
                applicationName = "Import-service-sample-app"
                correlationID = "Sample-job-0017-GetImportJobs_$i"
            } | ConvertTo-Json -Depth 10

            $response = $global:WebRequest.callPost($uri, $body)
            $global:WebRequest.checkIfSuccess($response)        

            $configurationUri = $global:Endpoints.documentConfigurationUri($importId)
            $response = $global:WebRequest.callPost($configurationUri, $jobConfigurationBody)
            $global:WebRequest.checkIfSuccess($response)
        }
        Write-Information -MessageData "$importJobsCount jobs created" -InformationAction Continue
    }

    Describe "Read import job collection (guid list) for particular workspace" {
        $uri = $global:Endpoints.importJobsForWorkspaceUri(0, $pageSize)
        
        $response = $global:WebRequest.callGet($uri)
        $totalCount = $response."Value"."TotalCount"
        Write-Information -MessageData "$totalCount jobs read" -InformationAction Continue
        Write-Information -MessageData "First $pageSize ImportJobIds:" -InformationAction Continue
        $jobs = $response."Value"."Jobs"

        foreach ($currentJob in $jobs) {
            Write-Information -MessageData "$currentJob" -InformationAction Continue
        }

        <# Expected output
            10 jobs read
            First 7 ImportJobIds:
            40335fb9-0c19-47df-8689-ec204cc30460
            316775d6-13eb-4f4f-92af-abeb63c6967a
            e02edeb1-bb1f-4425-8b23-d3aa2e1ac921
            950a75f5-daab-4a1d-9e80-a46c535cd9d3
            d30bc151-12dd-4588-9141-19ac6df204b7
            964f0223-d4dc-4d38-8f65-f718fc48594a
            05cb802a-e2fa-47eb-ba7d-ff48b56b6172
        #>
    }
}