// <copyright file="ResponseHelper.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>
namespace Relativity.Import.Samples.DotNetFrameworkClient.ImportSampleHelpers
{
	using System;
	using Relativity.Import.V1;

	/// <summary>
	/// Helper class for response.
	/// </summary>
	public static class ResponseHelper
	{
		/// <summary>
		/// Check if received response was with success (isSuccess is True).
		/// </summary>
		/// <param name="response">Response.</param>
		/// <param name="requestDescription">request description.</param>
		public static void EnsureSuccessResponse(Response response, string requestDescription = null)
		{
			ConsoleWriteLine(requestDescription, ConsoleColor.DarkGreen);

			if (response == null)
			{
				Console.WriteLine("Response is null");
				throw new Exception("Response is null");
			}

			if (response.IsSuccess == false)
			{
				var errorInfo = $"ErrorMessage: {response.ErrorMessage} ErrorCode: {response.ErrorCode}";
				Console.WriteLine($"Response.IsSuccess: {response.IsSuccess} {errorInfo}");

				throw new Exception($"Response failed: {errorInfo}");
			}

			Console.WriteLine($"Response.IsSuccess: {response.IsSuccess}");
		}

		private static void ConsoleWriteLine(string message, ConsoleColor color)
		{
			var saveColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ForegroundColor = saveColor;
		}
	}
}
