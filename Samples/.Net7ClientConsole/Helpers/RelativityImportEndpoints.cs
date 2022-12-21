// <copyright file="RelativityImportEndpoints.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.Net7Client.Helpers
{
	public static class RelativityImportEndpoints
	{
		// Import Job section
		public static string GetCreateImportUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}";

		public static string GetBeginJobUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/begin";

		public static string GetEndJobUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/end";

		public static string GetCancelJobUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/cancel";

		public static string GetImportDetailsUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/details";

		public static string GetImportProgressUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/progress";

		public static string GetImportUri(int workspaceId, int start, int length) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/jobs/?start={start}&length={length}";

		public static string GetImportSourcesUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources";

		// Document Configuration section
		public static string GetDocumentConfigurationUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/documents-configurations";

		// RDO Configuration section
		public static string GetRdoConfigurationUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/rdos-configurations";

		// Data Source section
		public static string GetImportSourceUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}";
		
		public static string GetImportSourceDetailsUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/details";
		
		public static string GetImportSourceProgressUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/progress";
		
		public static string GetItemErrorUri(int workspaceId, Guid importId, Guid sourceId, int start, int length) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/itemerrors?start={start}&length={length}";

	}
}
