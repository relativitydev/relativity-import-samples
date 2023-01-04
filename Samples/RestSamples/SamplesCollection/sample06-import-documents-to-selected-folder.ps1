#import
. "$global:rootDir\Helpers\EndpointsClass.ps1"
. "$global:rootDir\Helpers\WriteInformationClass.ps1"

$workspaceId = 1000000
$rootFolderId = 1000001
$loadFilePath = "C:\DefaultFileRepository\samples\load_file_03.dat"

$importId = New-Guid
$sourceId = New-Guid
$global:Endpoints = [Endpoints]::new($workspaceId)
$global:WriteInformation = [WriteInformation]::new()

Context "Sample06 Import documents to selected folder" {
    Describe "Create job" {
        $uri = $global:Endpoints.importJobCreateUri($importId)

        $body = @{
            applicationName = "Import-service-sample-app"
            correlationID = "Sample-job-0006"
        } | ConvertTo-Json -Depth 10
		
        $response = $global:WebRequest.callPost($uri, $body)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job $importId created" -InformationAction Continue
    }

    Describe "Create document configuration" {
        $uri = $global:Endpoints.documentConfigurationUri($importId)
        $field1 = @{
            ColumnIndex = 0
            Field = "Control Number"
            ContainsID = $false
            ContainsFilePath = $false
        }
        $fields = @($field1)

        $jobConfigurationBody = @{
            importSettings =
            @{
                Overlay = $null
                Native = @{
                    FilePathColumnIndex =  22
                    FileNameColumnIndex = 13
                }
                Image = $null
                Fields = @{
                    FieldMappings = $fields
                }
                Folder = @{
                    RootFolderID = $rootFolderId
                    FolderPathColumnIndex = 15
                }
            }
        } | ConvertTo-Json -Depth 10
        $response = $global:WebRequest.callPost($uri, $jobConfigurationBody)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job configuration created" -InformationAction Continue
    }

    Describe "Add Source" {
        $uri = $global:Endpoints.importSourceUri($importId, $sourceId)
        
        $loadFilePath = $loadFilePath.replace('\','\\')

        $dataSourceConfigurationBody = @{
            dataSourceSettings = @{
                path = $loadFilePath
                firstLineContainsColumnNames = $false
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
        #Data source progress: Total records: 2, Imported records: 2, Records with errors: 0
    }
}