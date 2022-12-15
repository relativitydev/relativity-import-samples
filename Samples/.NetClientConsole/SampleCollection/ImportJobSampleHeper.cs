using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1;
using Relativity.Import.Samples.NetClientConsole.HttpClientHelpers;
using Relativity.Import.Samples.NetClientConsole.RestClientHelpers;

namespace Relativity.Import.Samples.NetClientConsole.SampleCollection
{
    internal static class ImportJobSampleHeper
    {
        public static async Task EnsureSuccessResponse(HttpResponseMessage message)
        {
            var saveColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"{message.RequestMessage?.Method} { message.RequestMessage?.RequestUri?.PathAndQuery}");
            Console.ForegroundColor = saveColor;

			message.EnsureSuccessStatusCode();
            var response = await HttpClientHelper.DeserializeResponse<Response>(message);

            if (response == null)
            {
                var errInfo = "Deserialized model is null";

				Console.WriteLine(errInfo);
                throw new Exception(errInfo);
            }
            var errorInfo = $"ErrorMessage: {response.ErrorMessage ?? "null"} ErrorCode: {response.ErrorCode ?? "null"}";

            Console.WriteLine($"Response.IsSuccess: {response.IsSuccess} {errorInfo}");

            if (response.IsSuccess == false)
            {
                throw new Exception($"Response failed: {errorInfo}");
            }
        }


        internal static async Task<DataSourceState?> WaitImportDataSourceToBeCompleted(Func<Task<ResponseWrapper<DataSourceDetails>?>> funcAsync, int? timeout = null)
        {
            DataSourceState[] completedStates = { DataSourceState.Completed, DataSourceState.CompletedWithItemErrors, DataSourceState.Failed };
            DataSourceState? state = null;
            var timeoutTask = Task.Delay(timeout ?? Timeout.Infinite);
            do
            {
                await Task.Delay(1000);
                var wrappedValue = await funcAsync();
                var valueResponse = wrappedValue?.ValueResponse;
                if (valueResponse != null && valueResponse.IsSuccess)
                {
                    state = valueResponse.Value.State;
                    Console.WriteLine($"DataSource state: {state}");
                }
            }
            while (completedStates.All(x => x != state) && !timeoutTask.IsCompleted);

            return state;
        }
    }
}
