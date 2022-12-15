using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using static System.Net.WebRequestMethods;

namespace Relativity.Import.Samples.NetClientConsole.HttpClientHelpers
{
    internal static class HttpClientHelper
    {
        private static string baseAddress = "https://p-dv-vm-ole4vat/Relativity.REST/";
        private static string username = "relativity.admin@kcura.com";
        private static string password = "Test1234!";


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
                catch (JsonException)
                {
                    Console.WriteLine("Json is invalid.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"exception occurred during response deserialization: {ex.Message}");
                }
                return null;
            }
            return null;
        }

        internal static HttpClient CreateHttpClient()
        {
            var restClient = new HttpClient();
            restClient.DefaultRequestHeaders.Accept.Clear();
            restClient.DefaultRequestHeaders.Add("X-CSRF-Header", "-");
            restClient.BaseAddress = new Uri(baseAddress);

            var authenticationString = $"{username}:{password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            restClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            return restClient;
        }

    }
}
