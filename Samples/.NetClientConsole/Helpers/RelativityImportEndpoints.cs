﻿// <copyright file="RelativityImportEndpoints.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.Net7Client.Helpers
{
	public static class RelativityImportEndpoints
	{
		public static string GetCreateImportUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}";

		public static string GetBeginJobUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/begin";

		public static string GetEndJobUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/end";

		public static string GetDocumentConfigurationUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/documents-configurations";
		
		public static string GetImportSourcesUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}";
		
		public static string GetImportSourceDetailsUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/details";
		
		public static string GetImportSourceProgressUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/progress";

		public static string GetImportDetailsUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/details";
		
		public static string GetImportUri(int workspaceId, int size, int pageSize) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs";

		public static string GetRdoConfigurationUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}//rdos-configurations";
	}
}