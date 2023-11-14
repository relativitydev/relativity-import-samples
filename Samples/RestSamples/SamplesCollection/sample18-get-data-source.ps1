#import
. "$global:rootDir\Helpers\EndpointsClass.ps1"

$workspaceId = 1000000

$importId = New-Guid
$sourcesCount = 10
$pageSize = 7
$loadFilePath = "\\files\<TenantNumber>\Files\SampleDataSources\load_file"
$global:Endpoints = [Endpoints]::new($workspaceId)

Context "Sample18 Read Data sources for Import Job" {
    Describe "Create job" {
        $uri = $global:Endpoints.importJobCreateUri($importId)

        $body = @{
            applicationName = "Import-service-sample-app"
            correlationID = "Sample-job-0018"
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

    Describe "Add n data sources to the existing job." {
        for ($i = 0; $i -lt $sourcesCount; $i++) {
            $sourceId = New-Guid
            $uri = $global:Endpoints.importSourceAddUri($importId, $sourceId)
            $dataSourceConfigurationBody = @{
                dataSourceSettings = @{
                    path = $loadFilePath + $i
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
        Write-Information -MessageData "$sourcesCount sources added" -InformationAction Continue
    }

    Describe "Read collection of data sources for particular import job" {
        $uri = $global:Endpoints.importSourcesReadUri($importId)
        
        $response = $global:WebRequest.callGet($uri)
        $totalCount = $response."Value"."TotalCount"
        Write-Information -MessageData "$totalCount data sources read" -InformationAction Continue
        $sources = $response."Value"."Sources"

        foreach ($currentSource in $sources) {
            Write-Information -MessageData "$currentSource" -InformationAction Continue
        }

        <# Expected output
            10 data sources read
            05473585-22b5-4f66-9aa7-075e8285481f
            1056d461-7c2a-4320-ac1f-bfaf9ca69534
            1c2c34ea-0157-4335-9db8-c53d9c9106dc
            32b533fe-ab25-4f17-a030-2097283625a0
            49e8b0d5-8edc-4d30-a26a-2b9d4a869e34
            5e288417-52f3-418b-ba69-d457f92f268d
            71d2197f-0297-40cb-a597-1f9c584229d2
            7829ed2b-68e2-4aa8-9912-c8d3da2452aa
            91a74003-f015-4a1e-bff5-e2ea329e884f
            a0943e1d-cc11-42d0-94fb-5220147aa606
        #>
    }
}