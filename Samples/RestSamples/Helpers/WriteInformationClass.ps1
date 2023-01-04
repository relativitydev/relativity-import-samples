class WriteInformation {

    WriteInformation(){
    }

    getDataSourceProgress($uri) {
        $sourceProgresssResponse = $global:WebRequest.callGet($uri)
        $totalRecords = $sourceProgresssResponse."Value"."TotalRecords"
        $importedRecords = $sourceProgresssResponse."Value"."ImportedRecords"
        $erroredRecords = $sourceProgresssResponse."Value"."ErroredRecords"
        
        Write-Information -MessageData "Data source progress: Total records: $totalRecords, Imported records: $importedRecords, Records with errors: $erroredRecords" -InformationAction Continue
    }

    gateDataSourceErrors($importId, $sourceId) {
        $uri = $global:Endpoints.importSourceDetailsUri($importId, $sourceId) 
        $sourceDetailsResponse = $global:WebRequest.callGet($uri)
        $state = $sourceDetailsResponse."Value"."State"
        Write-Information -MessageData "Data source $sourceId state: $state" -InformationAction Continue

        if ($state -eq "Failed") {
            $jobLevelErrors = $sourceDetailsResponse."Value"."JobLevelErrors" | ConvertTo-Json -Depth 10
            Write-Information -MessageData "Data source failed due to errors: $jobLevelErrors" -InformationAction Continue
        }
        elseif ($state -eq "CompletedWithItemErrors") {
            $itemErrorsUri = $global:Endpoints.importSourcesItemErrorsUri($importId, $sourceId)
            $sourceItemErrorsResponse = $global:WebRequest.callGet($itemErrorsUri)
            $errors = $sourceItemErrorsResponse."Value"."Errors" | ConvertTo-Json -Depth 10
            Write-Information -MessageData "Data source item errors: $errors" -InformationAction Continue
        }
    }
}