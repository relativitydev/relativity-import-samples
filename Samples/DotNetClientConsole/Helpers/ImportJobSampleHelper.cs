// <copyright file="ImportJobSampleHelper.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.Import.Samples.DotNetClient.Helpers
{
	using Relativity.Import.V1;
	using Relativity.Import.V1.Models;
	using Relativity.Import.V1.Models.Sources;

	/// <summary>
	/// Helper class for import job sample.
	/// </summary>
	internal static class ImportJobSampleHelper
	{
		/// <summary>
		/// Make sure that response is success.
		/// </summary>
		/// <param name="message">Http response message.</param>
		/// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
		public static async Task EnsureSuccessResponse(HttpResponseMessage message)
		{
			ConsoleWriteLine($"{message.RequestMessage?.Method} {message.RequestMessage?.RequestUri?.PathAndQuery}", ConsoleColor.DarkGreen);

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

		/// <summary>
		/// Make sure that value response is success.
		/// </summary>
		/// <typeparam name="T">Generic type.</typeparam>
		/// <param name="message">Http response message.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
		public static async Task<ValueResponse<T>> EnsureSuccessValueResponse<T>(HttpResponseMessage message)
		{
			ConsoleWriteLine(
				$"{message.RequestMessage?.Method} {message.RequestMessage?.RequestUri?.PathAndQuery}",
				ConsoleColor.DarkGreen);

			message.EnsureSuccessStatusCode();
			var valueResponse = await HttpClientHelper.DeserializeResponse<ValueResponse<T>>(message);

			if (valueResponse == null)
			{
				var errorInfo = "Deserialized valueResponse model is null";

				Console.WriteLine(errorInfo);
				throw new Exception(errorInfo);
			}

			if (valueResponse.IsSuccess == false)
			{
				var errorInfo = $"ErrorMessage: {valueResponse.ErrorMessage} ErrorCode: {valueResponse.ErrorCode}";
				Console.WriteLine($"ValueResponse.IsSuccess: {valueResponse.IsSuccess} {errorInfo}");

				throw new Exception($"Response failed: {errorInfo}");
			}

			Console.WriteLine($"ValueResponse.IsSuccess: {valueResponse.IsSuccess}");

			return valueResponse;
		}

		/// <summary>
		/// Wait for complete import data source.
		/// </summary>
		/// <param name="funcAsync">Value response of data source details.</param>
		/// <param name="timeout">Timeout.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
		internal static async Task<DataSourceState?> WaitImportDataSourceToBeCompleted(Func<Task<ValueResponse<DataSourceDetails>?>> funcAsync, int? timeout = null)
		{
			DataSourceState[] completedStates = { DataSourceState.Completed, DataSourceState.CompletedWithItemErrors, DataSourceState.Failed };
			DataSourceState? state = null;
			var timeoutTask = Task.Delay(timeout ?? Timeout.Infinite);
			do
			{
				await Task.Delay(1000);
				var valueResponse = await funcAsync();
				if (valueResponse is { IsSuccess: true })
				{
					state = valueResponse.Value.State;
					Console.WriteLine($"DataSource state: {state}");
				}
			}
			while (completedStates.All(x => x != state) && !timeoutTask.IsCompleted);

			return state;
		}

		/// <summary>
		/// Wait for import job to be finished.
		/// </summary>
		/// <param name="funcAsync">Value response of import details.</param>
		/// <param name="timeout">Timeout.</param>
		/// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
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
					Console.WriteLine($"Import job state: {state}");
				}
			}
			while (!isFinished && !timeoutTask.IsCompleted);

			return state;
		}

		/// <summary>
		/// Console write line.
		/// </summary>
		/// <param name="message">Message to write.</param>
		/// <param name="color">Color.</param>
		internal static void ConsoleWriteLine(string message, ConsoleColor color)
		{
			var saveColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ForegroundColor = saveColor;
		}
	}
}
