#import
. "$global:rootDir\Helpers\EndpointsClass.ps1"

$workspaceId = 1000000

$importId = New-Guid
$rdoArtifactTypeID = 1000027
$global:Endpoints = [Endpoints]::new($workspaceId)

Context "Sample15 Read RDO settings" {
    Describe "Create job" {
        $uri = $global:Endpoints.importJobCreateUri($importId)

        $body = @{
            applicationName = "Import-service-sample-app"
            correlationID = "Sample-job-import-0015"
        } | ConvertTo-Json -Depth 10
		
        $response = $global:WebRequest.callPost($uri, $body)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job $importId created" -InformationAction Continue
    }

    Describe "Create RDO configuration" {
        $uri = $global:Endpoints.rdoConfigurationUri($importId)
        $field1 = @{
            ColumnIndex = 2
            Field = "RdoName"
            ContainsID = $false
            ContainsFilePath = $false
        }
        $field2 = @{
            ColumnIndex = 3
            Field = "RdoValue"
            ContainsID = $false
            ContainsFilePath = $false
        }
        $fields = @($field1, $field2)

        $jobConfigurationBody = @{
            importSettings =
            @{
                Overlay = $null
                Fields = @{
                    FieldMappings = $fields
                }
                "Rdo" = @{
                    ArtifactTypeID = $rdoArtifactTypeID
                    ParentColumnIndex = 4
                }
            }
        } | ConvertTo-Json -Depth 10
        $response = $global:WebRequest.callPost($uri, $jobConfigurationBody)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job configuration created" -InformationAction Continue
    }

    Describe "Read RDO configuration" {
        $uri = $global:Endpoints.rdoConfigurationUri($importId)	
        $value = $global:WebRequest.callGet($uri)."Value" | ConvertTo-Json -Depth 10
        Write-Information -MessageData "Configuration: $value" -InformationAction Continue
    }

    <# Expected output
        Configuration:  {
            "Fields": {
                "FieldMappings": [
                {
                    "ColumnIndex": 2,
                    "Field": "RdoName",
                    "ContainsID": false,
                    "ContainsFilePath": false
                },
                {
                    "ColumnIndex": 3,
                    "Field": "RdoValue",
                    "ContainsID": false,
                    "ContainsFilePath": false
                }
                ]
            },
            "Rdo": {
                "ArtifactTypeID": 1000027,
                "ParentColumnIndex": 4
            }
        }
    #>
}