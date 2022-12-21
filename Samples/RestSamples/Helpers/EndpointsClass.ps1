class Endpoints {
    [String] $documentConfigurationUri
    [String] $createImportJobUri
    [String] $beginImportJobUri
    [String] $endImportJobUri
    [String] $importJobDetailsUri
    [String] $importSourcesUri
    [String] $importSourceDetailsUri
    [String] $importSourceProgressUri

    Endpoints($workspaceId, $importId, $sourceId){
        $baseAddress = "Relativity.REST/api/import-service/v1/workspaces/$workspaceId"
        $this.documentConfigurationUri = "$baseAddress/import-jobs/$importId/documents-configurations"
        $this.createImportJobUri = "$baseAddress/import-jobs/$importId"
        $this.beginImportJobUri = "$baseAddress/import-jobs/$importId/begin"
        $this.endImportJobUri = "$baseAddress/import-jobs/$importId/end"
        $this.importJobDetailsUri = "$baseAddress/import-jobs/$importId/details"
        $this.importSourcesUri = "$baseAddress/import-jobs/$importId/sources/$sourceId"
        $this.importSourceDetailsUri = "$baseAddress/import-jobs/$importId/sources/$sourceId/details"
        $this.importSourceProgressUri = "$baseAddress/import-jobs/$importId/sources/$sourceId/progress"
    }
}