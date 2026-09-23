using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Autouchet_Bot.Services
{
    public class CreateUserRequest
    {
        public long MaxUserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
    }

    public class TaxSummaryDto
    {
        public long MaxUserId { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public class BackendApiClient
    {
        private readonly HttpClient _httpClient;

        public BackendApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            string baseUrl = Environment.GetEnvironmentVariable("API_URL") ?? "http://localhost:5000";
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<bool> CreateUserAsync(long maxUserId, string firstName)
        {
            try
            {
                var payload = new CreateUserRequest
                {
                    MaxUserId = maxUserId,
                    FirstName = firstName
                };
                var response = await _httpClient.PostAsJsonAsync("api/Users", payload);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error CreateUserAsync]: {ex.Message}");
                return false;
            }
        }

        public async Task<List<TaxSummaryDto>> GetTaxSummaryAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<TaxSummaryDto>>("api/Users/tax-summary");
                return response ?? new List<TaxSummaryDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error GetTaxSummaryAsync]: {ex.Message}");
                return new List<TaxSummaryDto>();
            }
        }
    }
}