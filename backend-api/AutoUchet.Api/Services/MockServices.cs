namespace AutoUchet.Api.Services
{
    public class MockServices
    {
        public static string CreateMockFnsUrl()
        {
            var mockFnsId = Guid.NewGuid().ToString();
            var mockFnsUrl = $"https://mock-fns.local/receipt/{mockFnsId}";
            return mockFnsUrl;
        }
    }
}
