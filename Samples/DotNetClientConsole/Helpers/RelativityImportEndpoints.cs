// <copyright file="RelativityImportEndpoints.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetClient.Helpers
{
	public static class RelativityImportEndpoints
	{
		// Import Job section
		public static string GetImportJobCreateUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}";

		public static string GetImportJobBeginUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/begin";

		public static string GetImportJobEndUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/end";

		public static string GetImportJobCancelUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/cancel";

		public static string GetImportJobDetailsUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/details";

		public static string GetImportJobProgressUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/progress";

		public static string GetImportJobsForWorkspaceUri(int workspaceId, int start, int length) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/jobs/?start={start}&length={length}";

		public static string GetImportSourcesForJobUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources";

		// Document Configuration section
		public static string GetDocumentConfigurationUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/documents-configurations";

		// RDO Configuration section
		public static string GetRdoConfigurationUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/rdos-configurations";

		// Data Source section
		public static string GetImportSourceUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}";
		
		public static string GetImportSourceDetailsUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/details";
		
		public static string GetImportSourceProgressUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/progress";
		
		public static string GetImportSourceItemErrorsUri(int workspaceId, Guid importId, Guid sourceId, int start, int length) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/itemerrors?start={start}&length={length}";

	}
}
