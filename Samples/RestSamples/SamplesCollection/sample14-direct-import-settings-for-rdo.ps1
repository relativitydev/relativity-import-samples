#import
. "$global:rootDir\Helpers\EndpointsClass.ps1"
. "$global:rootDir\Helpers\WriteInformationClass.ps1"

$workspaceId = 1000000
$loadFilePath = "\\files\<TenantNumber>\Files\SampleDataSources\rdo_load_file_02.dat"

$importId = New-Guid
$sourceId = New-Guid
$rdoArtifactTypeID = 1000027
$global:Endpoints = [Endpoints]::new($workspaceId)
$global:WriteInformation = [WriteInformation]::new()

# Example of import  Relativity Dynamic Object (RDO).
# NOTE: Existing RDO "Domain" is used in this example. Please insert documents from sample01 first.
Context "Sample14 Direct import settings for RDO" {
    Describe "Create job" {
        $uri = $global:Endpoints.importJobCreateUri($importId)

        $body = @{
            applicationName = "Import-service-sample-app"
            correlationID = "Sample-job-import-00014"
        } | ConvertTo-Json -Depth 10
		
        $response = $global:WebRequest.callPost($uri, $body)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job $importId created" -InformationAction Continue
    }

    Describe "Create RDO configuration" {
        $uri = $global:Endpoints.rdoConfigurationUri($importId)
        $field1 = @{
            ColumnIndex = 0
            Field = "Name"
            ContainsID = $false
            ContainsFilePath = $false
        }
        # Use sample01 and load_file_01.dat first to import document. The following fields have reference to these documents.
		# If you do not use these fields, please just comment them.
        $field2 = @{
            ColumnIndex = 3
            Field = "Domains (Email CC)"
            ContainsID = $false
            ContainsFilePath = $false
        }
        $field3 = @{
            ColumnIndex = 4
            Field = "Domains (Email From)"
            ContainsID = $false
            ContainsFilePath = $false
        }
        $field4 = @{
            ColumnIndex = 5
            Field = "Domains (Email To)"
            ContainsID = $false
            ContainsFilePath = $false
        }
        $fields = @($field1, $field2, $field3, $field4)

        $jobConfigurationBody = @{
            importSettings =
            @{
                Overlay = @{
                    Mode = 3
                    KeyField = "Name"
                    MultiFieldOverlayBehaviour = 1
                }
                Fields = @{
                    FieldMappings = $fields
                }
                "Rdo" = @{
                    ArtifactTypeID = $rdoArtifactTypeID
                    ParentColumnIndex = $null
                }
            }
        } | ConvertTo-Json -Depth 10
        $response = $global:WebRequest.callPost($uri, $jobConfigurationBody)
        $global:WebRequest.checkIfSuccess($response)
        Write-Information -MessageData "Job configuration created" -InformationAction Continue
    }

    Describe "Add Source" {
        $uri = $global:Endpoints.importSourceAddUri($importId, $sourceId)
        $dataSourceConfigurationBody = @{
            dataSourceSettings = @{
                path = $loadFilePath
                NewLineDelimiter = '#'
				ColumnDelimiter = '|'
				QuoteDelimiter = '^'
				MultiValueDelimiter = '$'
				NestedValueDelimiter = '&'
				Encoding = $null
				CultureInfo = "en-us"
				EndOfLine = 0
				FirstLineContainsColumnNames = $true
				StartLine = 0
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
        #Data source progress: Total records: 3, Imported records: 3, Records with errors: 0
    }
}