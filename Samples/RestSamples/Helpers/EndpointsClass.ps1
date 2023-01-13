class Endpoints {
    [String] $baseAddress

    Endpoints($workspaceId){
        $this.baseAddress = "Relativity.REST/api/import-service/v1/workspaces/$workspaceId"
    }

    [String] importJobCreateUri($importId){
        return  $this.baseAddress + "/import-jobs/$importId"
    }

    [String] documentConfigurationUri($importId){
        return $this.baseAddress + "/import-jobs/$importId/documents-configurations"
    }

    [String] rdoConfigurationUri($importId){
        return $this.baseAddress + "/import-jobs/$importId/rdos-configurations"
    }

    [String] importJobBeginUri($importId){
        return $this.baseAddress + "/import-jobs/$importId/begin"
    }

    [String] importJobEndUri($importId){
        return $this.baseAddress + "/import-jobs/$importId/end"
    }

    [String] importJobCancelUri($importId){
        return $this.baseAddress + "/import-jobs/$importId/cancel"
    }

    [String] importJobDetailsUri($importId){
        return $this.baseAddress + "/import-jobs/$importId/details"
    }

    [String] importJobProgressUri($importId){
        return $this.baseAddress + "/import-jobs/$importId/progress"
    }

    [String] importSourcesReadUri($importId){
        return $this.baseAddress + "/import-jobs/$importId/sources"
    }

    [String] importSourceAddUri($importId, $sourceId){
        return $this.baseAddress + "/import-jobs/$importId/sources/$sourceId"
    }

    [String] importSourceDetailsUri($importId, $sourceId){
        return $this.baseAddress + "/import-jobs/$importId/sources/$sourceId/details"
    }

    [String] importSourceProgressUri($importId, $sourceId){
        return $this.baseAddress + "/import-jobs/$importId/sources/$sourceId/progress"
    }

    [String] importSourceItemErrorsUri($importId, $sourceId){
        return $this.baseAddress + "/import-jobs/$importId/sources/$sourceId/itemerrors"
    }

    [String] importJobsForWorkspaceUri($start, $length){
        return $this.baseAddress + "/import-jobs/jobs/?start=$start&length=$length"
    }
}