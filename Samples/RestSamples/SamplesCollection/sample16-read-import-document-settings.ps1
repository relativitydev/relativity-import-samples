#import
. "$global:rootDir\Helpers\EndpointsClass.ps1"

$importId = New-Guid
$workspaceId = 1000000
$global:Endpoints = [Endpoints]::new($workspaceId)

Context "Sample16 Read document settings" {
    Describe "Create job" {
        $uri = $global:Endpoints.importJobCreateUri($importId)

        $body = @{
            applicationName = "Import-service-sample-app"
            correlationID = "Sample-job-0016"
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
        $response = $global:WebRequest.callPost($uri, $jobConfigurationBody)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job configuration created" -InformationAction Continue
    }

    Describe "Read document configuration" {
        $uri = $global:Endpoints.documentConfigurationUri($importId)
        $value = $global:WebRequest.callGet($uri)."Value" | ConvertTo-Json -Depth 10
        Write-Information -MessageData "Configuration: $value" -InformationAction Continue
    }

    <#Expected output
    Configuration: {
        "Native": {
            "FilePathColumnIndex": 22,
            "FileNameColumnIndex": 13
        },
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
        }
    }
#>
}