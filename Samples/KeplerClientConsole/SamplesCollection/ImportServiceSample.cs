// <copyright file="ImportServiceSample.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.NetFrameworkClient.SamplesCollection
{
	using System;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;

	using Relativity.Import.Samples.NetFrameworkClient.ImportSampleHelpers;
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

		/// <summary>
		/// Initializes a new instance of the <see cref="ImportServiceSample"/> class.
		/// </summary>
		public ImportServiceSample()
		{
			this._serviceFactory = KeplerProxyHelper.GetServiceFactory();
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
	}
}
