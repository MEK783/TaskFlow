using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskFlowAPI.Models;
using TaskFlowAPI.Tests.Fixtures;
using Xunit;

namespace TaskFlowAPI.Tests.Controllers
{
    public class AuthenticationControllerTests
    {
        private static Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions ClientOptions => new()
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        };

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenModelInvalid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequest
            {
                Username = "",
                Password = "123",
                InviteCode = "SHORT"
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenInviteInvalid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequest
            {
                Username = "newuser",
                Password = TestDataSeeder.DefaultPasswordSha512,
                InviteCode = TestDataSeeder.ExpiredInviteCode
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUsernameExists()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequest
            {
                Username = "alice",
                Password = TestDataSeeder.DefaultPasswordSha512,
                InviteCode = TestDataSeeder.ValidInviteCode
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenValid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequest
            {
                Username = "charlie",
                Password = TestDataSeeder.DefaultPasswordSha512,
                InviteCode = TestDataSeeder.ValidInviteCode
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(payload.GetProperty("success").GetBoolean());
            Assert.Equal("Registration successful", payload.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenModelInvalid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PostAsJsonAsync("/api/v1.0/auth/login", new LoginRequest
            {
                Username = "",
                Password = "",
                RememberMe = false
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsInvalid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PostAsJsonAsync("/api/v1.0/auth/login", new LoginRequest
            {
                Username = "alice",
                Password = "invalid-hash",
                RememberMe = false
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsOkAndSetsCookie_WhenValid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PostAsJsonAsync("/api/v1.0/auth/login", new LoginRequest
            {
                Username = "alice",
                Password = TestDataSeeder.DefaultPasswordSha512,
                RememberMe = false
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var setCookie = response.Headers.TryGetValues("Set-Cookie", out var values)
                ? string.Join(";", values)
                : string.Empty;
            Assert.Contains("TaskFlowRefreshToken", setCookie);
        }

        [Fact]
        public async Task Logout_ReturnsOk_WhenCalled()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PostAsync("/api/v1.0/auth/logout", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GenerateInvite_ReturnsUnauthorized_WhenNoSession()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PostAsJsonAsync("/api/v1.0/auth/generate-invite", new GenerateInviteRequest
            {
                ExpirationDays = 10
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GenerateInvite_ReturnsUnauthorized_WhenSessionInvalid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", "TaskFlowRefreshToken=invalid-token");

            var response = await client.PostAsJsonAsync("/api/v1.0/auth/generate-invite", new GenerateInviteRequest
            {
                ExpirationDays = 10
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GenerateInvite_ReturnsOk_WhenSessionValid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PostAsJsonAsync("/api/v1.0/auth/generate-invite", new GenerateInviteRequest
            {
                ExpirationDays = 10
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(payload.GetProperty("success").GetBoolean());
            Assert.True(payload.TryGetProperty("invite", out _));
        }

        [Fact]
        public async Task Refresh_ReturnsUnauthorized_WhenNoCookie()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PostAsync("/api/v1.0/auth/refresh", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Refresh_ReturnsUnauthorized_WhenTokenExpired()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ExpiredRefreshToken}");

            var response = await client.PostAsync("/api/v1.0/auth/refresh", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Refresh_ReturnsOk_WhenTokenActive()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PostAsync("/api/v1.0/auth/refresh", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(payload.GetProperty("success").GetBoolean());
        }
    }
}
