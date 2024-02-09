// <copyright file="RelativityImportEndpoints.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetClient.Helpers
{
	/// <summary>
	///  Class containing relativity import endpoints.
	/// </summary>
	public static class RelativityImportEndpoints
	{
		// Import Job section

		/// <summary>
		/// Get import job create uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <returns>String representing the uri to create import job.</returns>
		public static string GetImportJobCreateUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}";

		/// <summary>
		/// Get import job begin uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <returns>String representing the uri to begin import job.</returns>
		public static string GetImportJobBeginUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/begin";

		/// <summary>
		/// Get import job end uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <returns>String representing the uri to end import job.</returns>
		public static string GetImportJobEndUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/end";

		/// <summary>
		/// Get import job cancel uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <returns>String representing the uri to cancel import job.</returns>
		public static string GetImportJobCancelUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/cancel";

		/// <summary>
		/// Get import job details uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <returns>String representing the uri to get import job details.</returns>
		public static string GetImportJobDetailsUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/details";

		/// <summary>
		/// Get import job progress uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <returns>String representing the uri to get import job progress.</returns>
		public static string GetImportJobProgressUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/progress";

		/// <summary>
		/// Get import jobs for workspace uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="start">Start number.</param>
		/// <param name="length">Number of jobs to get.</param>
		/// <returns>String representing the uri to get import jobs for specified workspace.</returns>
		public static string GetImportJobsForWorkspaceUri(int workspaceId, int start, int length) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/jobs/?start={start}&length={length}";

		/// <summary>
		/// Get import sources for job uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <returns>String representing the uri to get import sources for specified job.</returns>
		public static string GetImportSourcesForJobUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources";

		// Document Configuration section

		/// <summary>
		/// Get document configuration uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <returns>String representing the uri to get document configuration.</returns>
		public static string GetDocumentConfigurationUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/documents-configurations";

		// RDO Configuration section

		/// <summary>
		/// Get RDO configuration uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <returns>String representing the uri to get RDO configuration.</returns>
		public static string GetRdoConfigurationUri(int workspaceId, Guid importId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/rdos-configurations";

		// Data Source section

		/// <summary>
		/// Get import source uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <param name="sourceId">Source ID.</param>
		/// <returns>String representing the uri to get specified import source.</returns>
		public static string GetImportSourceUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}";

		/// <summary>
		/// Get import source details uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <param name="sourceId">Source ID.</param>
		/// <returns>String representing the uri to get details of specified import source.</returns>
		public static string GetImportSourceDetailsUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/details";

		/// <summary>
		/// Get import source progress uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <param name="sourceId">Source ID.</param>
		/// <returns>String representing the uri to get progress of specified import source.</returns>
		public static string GetImportSourceProgressUri(int workspaceId, Guid importId, Guid sourceId) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/progress";

		/// <summary>
		/// Get import source item errors uri.
		/// </summary>
		/// <param name="workspaceId">Workspace ID.</param>
		/// <param name="importId">Import job ID.</param>
		/// <param name="sourceId">Source ID.</param>
		/// <returns>String representing the uri to get errors reported during import for a single source.</returns>
		public static string GetImportSourceItemErrorsUri(int workspaceId, Guid importId, Guid sourceId, int start, int length) => $"api/import-service/v1/workspaces/{workspaceId}/import-jobs/{importId}/sources/{sourceId}/itemerrors?start={start}&length={length}";
	}
}
