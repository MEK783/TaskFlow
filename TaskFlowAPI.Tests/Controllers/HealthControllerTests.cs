using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskFlowAPI.Tests.Fixtures;
using Xunit;

namespace TaskFlowAPI.Tests.Controllers
{
    public class HealthControllerTests
    {
        [Fact]
        public async Task Get_ReturnsOkWithMessageAndTimestamp()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            var response = await client.GetAsync("/api/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(payload.TryGetProperty("message", out _));
            Assert.True(payload.TryGetProperty("timestamp", out _));
        }
    }
}
