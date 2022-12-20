// <copyright file="ImportJobSampleHelper.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;

namespace Relativity.Import.Samples.Net7Client.Helpers
{
	internal static class ImportJobSampleHelper
	{
		public static async Task EnsureSuccessResponse(HttpResponseMessage message)
		{
			var saveColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine($"{message.RequestMessage?.Method} {message.RequestMessage?.RequestUri?.PathAndQuery}");
			Console.ForegroundColor = saveColor;

			message.EnsureSuccessStatusCode();
			var response = await HttpClientHelper.DeserializeResponse<Response>(message);

			if (response == null)
			{
				var errorInfo = "Deserialized response model is null";

				Console.WriteLine(errorInfo);
				throw new Exception(errorInfo);
			}

			if (response.IsSuccess == false)
			{
				var errorInfo = $"ErrorMessage: {response.ErrorMessage} ErrorCode: {response.ErrorCode}";
				Console.WriteLine($"Response.IsSuccess: {response.IsSuccess} {errorInfo}");

				throw new Exception($"Response failed: {errorInfo}");
			}

			Console.WriteLine($"Response.IsSuccess: {response.IsSuccess}");
		}


		public static async Task<ValueResponse<T>> EnsureSuccessValueResponse<T>(HttpResponseMessage message)
		{
			var saveColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine($"{message.RequestMessage?.Method} {message.RequestMessage?.RequestUri?.PathAndQuery}");
			Console.ForegroundColor = saveColor;

			message.EnsureSuccessStatusCode();
			var response = await HttpClientHelper.DeserializeResponse<ValueResponse<T>>(message);

			if (response == null)
			{
				var errorInfo = "Deserialized response model is null";

				Console.WriteLine(errorInfo);
				throw new Exception(errorInfo);
			}

			if (response.IsSuccess == false)
			{
				var errorInfo = $"ErrorMessage: {response.ErrorMessage} ErrorCode: {response.ErrorCode}";
				Console.WriteLine($"Response.IsSuccess: {response.IsSuccess} {errorInfo}");

				throw new Exception($"Response failed: {errorInfo}");
			}

			Console.WriteLine($"Response.IsSuccess: {response.IsSuccess}");
			
			return response;

		}

		internal static async Task<DataSourceState?> WaitImportDataSourceToBeCompleted(Func<Task<ValueResponse<DataSourceDetails>?>> funcAsync, int? timeout = null)
		{
			DataSourceState[] completedStates = { DataSourceState.Completed, DataSourceState.CompletedWithItemErrors, DataSourceState.Failed };
			DataSourceState? state = null;
			var timeoutTask = Task.Delay(timeout ?? Timeout.Infinite);
			do
			{
				await Task.Delay(1000);
				var valueResponse = await funcAsync();
				if (valueResponse is {IsSuccess: true})
				{
					state = valueResponse.Value.State;
					Console.WriteLine($"DataSource state: {state}");
				}
			}
			while (completedStates.All(x => x != state) && !timeoutTask.IsCompleted);

			return state;
		}

		internal static async Task<ImportState?> WaitImportJobToBeFinished(Func<Task<ValueResponse<ImportDetails>?>> funcAsync, int? timeout = null)
		{
			var isFinished = false;
			ImportState? state = null;
			var timeoutTask = Task.Delay(timeout ?? Timeout.Infinite);
			do
			{
				await Task.Delay(1000);
				var valueResponse = await funcAsync();
				if (valueResponse is { IsSuccess: true })
				{
					isFinished = valueResponse.Value.IsFinished;
					state = valueResponse.Value?.State;
					Console.WriteLine($"DataSource state: {state}");
				}
			}
			while (!isFinished && !timeoutTask.IsCompleted);

			return state;
		}
	}
}
