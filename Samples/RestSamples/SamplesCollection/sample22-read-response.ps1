#import
. "$global:rootDir\Helpers\EndpointsClass.ps1"

$importId = New-Guid
$sourceId = New-Guid
$workspaceId = 1000000
$global:Endpoints = [Endpoints]::new($workspaceId)

Context "Sample22 Read response with errors" {
    Describe "Create job" {
        $uri = $global:Endpoints.importJobCreateUri($importId)

        $body = @{
            applicationName = "Import-service-sample-app"
            correlationID = "Sample-job-0022"
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
            }
        }'
        $response = $global:WebRequest.callPost($uri, $jobConfigurationBody)
        if($response."IsSuccess" -eq $false)
        {
            $errorCode = $response."ErrorCode"
            $message = $response."ErrorMessage"
            Write-Information -MessageData "$errorCode" -InformationAction Continue
            Write-Information -MessageData "$message" -InformationAction Continue
        }
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
        if($response."IsSuccess" -eq $false)
        {
            $errorCode = $response."ErrorCode"
            $message = $response."ErrorMessage"
            Write-Information -MessageData "$errorCode" -InformationAction Continue
            Write-Information -MessageData "$message" -InformationAction Continue
        }
    }
    
    # Expected output
    # C.CR.VLD.2001
    # Cannot create Job Configuration. Invalid import job configuration: Nothing is imported.
    # S.CR.VLD.3001
    # Cannot create Data Source. Invalid load file settings: LoadFilePath cannot be empty.

}