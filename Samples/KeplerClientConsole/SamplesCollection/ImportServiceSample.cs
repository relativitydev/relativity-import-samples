// <copyright file="ImportServiceSample.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.dotNetWithKepler.SamplesCollection
{
	using System;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
    using Relativity.Import.V1;
    using Relativity.Import.V1.Models;
	using Relativity.Import.V1.Models.Sources;
	using Relativity.Services.ServiceProxy;

	/// <summary>
	///  Class containing examples of using import service SDK.
	/// </summary>
	public partial class ImportServiceSample
	{
		private readonly IServiceFactory _serviceFactory;
		private readonly string _host;
		private readonly string _username;
		private readonly string _password;

		/// <summary>
		/// Initializes a new instance of the <see cref="ImportServiceSample"/> class.
		/// </summary>
		/// <param name="host">relativity host.</param>
		/// <param name="username">username.</param>
		/// <param name="password">password.</param>
		public ImportServiceSample(string host, string username, string password)
		{
			this._host = host;
			this._username = username;
			this._password = password;

			this._serviceFactory = this.GetServiceFactory();
		}

		/// <summary>
		/// Helper method create ServiceFactory class.
		/// Use the ServiceFactory class to create proxies for Kepler services. See documentation for details:
		/// https://platform.relativity.com/RelativityOne/Content/Kepler_framework/Proxies_and_authentication.htm#Service .
		/// </summary>
		/// <returns>ServiceFactory instance.</returns>
		protected IServiceFactory GetServiceFactory()
		{
			Uri relativityRestUri = new Uri($"{this._host}relativity.rest/api");
			Credentials credentials = new UsernamePasswordCredentials(this._username, this._password);

			ServiceFactorySettings settings = new ServiceFactorySettings(relativityRestUri, credentials);

			return new ServiceFactory(settings);
		}

		/// <summary>
		/// Check if received response was with success (isSuccess is True).
		/// </summary>
		/// <param name="response">Response.</param>
		/// <param name="requestDescription">request description.</param>
		/// <returns>bool indicating the received response has IsSuccess.</returns>
		protected bool IsPreviousResponseWithSuccess(Response response, string requestDescription = null)
		{
			string errorInfo = response.IsSuccess ? string.Empty : $"ErrorMessage: {response.ErrorMessage} ErrorCode: {response.ErrorCode}";
			Console.WriteLine($"{requestDescription} Response.IsSuccess: {response.IsSuccess} {errorInfo}");

			return response.IsSuccess;
		}

		private async Task<DataSourceState?> WaitToStatusChange(DataSourceState targetStatus, Func<Task<ValueResponse<DataSourceDetails>>> funcAsync, int? timeout = null)
		{
			DataSourceState? state;
			var timeoutTask = Task.Delay(timeout ?? Timeout.Infinite);
			do
			{
				await Task.Delay(1000);
				state = funcAsync().Result.Value.State;
				Console.WriteLine($"DataSource status: {state}");
			}
			while (state != targetStatus && !timeoutTask.IsCompleted);

			return state;
		}

		private async Task<DataSourceState?> WaitImportDataSourceToBeCompleted(Func<Task<ValueResponse<DataSourceDetails>>> funcAsync, int? timeout = null)
		{
			DataSourceState[] completedStates = { DataSourceState.Completed, DataSourceState.CompletedWithItemErrors, DataSourceState.Failed };
			DataSourceState? state = null;
			var timeoutTask = Task.Delay(timeout ?? Timeout.Infinite);
			do
			{
				await Task.Delay(1000);
				var response = await funcAsync();
				if (response.IsSuccess)
				{
					state = response.Value.State;
					Console.WriteLine($"DataSource state: {state}");
				}
			}
			while (completedStates.All(x => x != state) && !timeoutTask.IsCompleted);

			return state;
		}

		private async Task<ImportState?> WaitImportJobToBeCompleted(Func<Task<ValueResponse<ImportDetails>>> funcAsync, int? timeout = null)
		{
			ImportState[] completedStates = { ImportState.Completed, ImportState.Failed };
			ImportState? state = null;
			var timeoutTask = Task.Delay(timeout ?? Timeout.Infinite);
			do
			{
				await Task.Delay(1000);
				var response = await funcAsync();
				if (response.IsSuccess)
				{
					state = response.Value.State;
					Console.WriteLine($"Import job state: {state}");
				}
			}
			while (completedStates.All(x => x != state) && !timeoutTask.IsCompleted);

			return state;
		}

		private void DisplayExecutionInfo(string methodName) => Console.WriteLine($" Executing: {methodName}");
	}
}
