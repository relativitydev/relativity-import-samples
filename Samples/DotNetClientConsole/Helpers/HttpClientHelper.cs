// <copyright file="HttpClientHelper.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Relativity.Import.Samples.DotNetClient.Helpers
{
	internal static class HttpClientHelper
	{
		internal static StringContent SerializeContent<T>(T model)
		{
			var requestModel = new
			{
				model
			};

			var json = JsonSerializer.Serialize(requestModel);
			return new StringContent(json, Encoding.UTF8, "application/json");
		}

		internal static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response) where T : class
		{
			if (response.IsSuccessStatusCode)
			{
				try
				{
					return await response.Content.ReadFromJsonAsync<T>();
				}
				catch (JsonException jsonException)
				{
					Console.WriteLine($"JsonExceptionJson occurred during response deserialization: {jsonException}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Exception occurred during response deserialization: {ex.Message}");
				}
				return null;
			}
			return null;
		}

		internal static HttpClient CreateHttpClient()
		{
			var restClient = new HttpClient
			{
				BaseAddress = new Uri(RelativityUserSettings.BaseAddress),
			};

			// See Relativity REST API HTTP headers documentation: https://platform.relativity.com/RelativityOne/Content/REST_API/HTTP_headers.htm
			restClient.DefaultRequestHeaders.Accept.Clear();
			restClient.DefaultRequestHeaders.Add("X-CSRF-Header", "-");
			restClient.DefaultRequestHeaders.Authorization = Authorization.GetBasicAuthenticationHeaderValue();

			return restClient;
		}

	}
}
