using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskFlowAPI.Models;
using TaskFlowAPI.Tests.Fixtures;
using Xunit;

namespace TaskFlowAPI.Tests.Controllers
{
    public class TasksControllerTests
    {
        private static Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions ClientOptions => new()
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        };

        [Fact]
        public async Task GetAllTasks_ReturnsUnauthorized_WhenNoSession()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.GetAsync("/api/v1.0/tasks");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAllTasks_ReturnsOk_WhenSessionValid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.GetAsync("/api/v1.0/tasks");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(payload.GetProperty("success").GetBoolean());
            Assert.Equal(2, payload.GetProperty("tasks").GetArrayLength());
        }

        [Fact]
        public async Task GetTaskById_ReturnsUnauthorized_WhenNoSession()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.GetAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOpenId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetTaskById_ReturnsNotFound_WhenTaskNotOwned()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.GetAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOtherUserId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTaskById_ReturnsOk_WhenOwned()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.GetAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOpenId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(TestDataSeeder.TaskOpenId, payload.GetProperty("task").GetProperty("id").GetInt32());
        }

        [Fact]
        public async Task GetTasksByStatus_ReturnsBadRequest_WhenStatusInvalid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.GetAsync("/api/v1.0/tasks/status/9999");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTasksByStatus_ReturnsOk_WhenStatusValid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.GetAsync($"/api/v1.0/tasks/status/{TestDataSeeder.StatusTodoId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(payload.GetProperty("success").GetBoolean());
            Assert.Equal(1, payload.GetProperty("tasks").GetArrayLength());
        }

        [Fact]
        public async Task CreateTask_ReturnsBadRequest_WhenModelInvalid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PostAsJsonAsync("/api/v1.0/tasks", new CreateTaskRequest
            {
                TaskName = "",
                StatusId = 0
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateTask_ReturnsBadRequest_WhenStatusInvalid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PostAsJsonAsync("/api/v1.0/tasks", new CreateTaskRequest
            {
                TaskName = "New Task",
                TaskDescription = "Something",
                StatusId = 9999
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateTask_ReturnsCreated_WhenValid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PostAsJsonAsync("/api/v1.0/tasks", new CreateTaskRequest
            {
                TaskName = "Brand New",
                TaskDescription = "Create",
                StatusId = TestDataSeeder.StatusTodoId
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(payload.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task UpdateTask_ReturnsUnauthorized_WhenNoSession()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PutAsJsonAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOpenId}", new UpdateTaskRequest
            {
                TaskName = "Updated"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateTask_ReturnsNotFound_WhenTaskNotOwned()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PutAsJsonAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOtherUserId}", new UpdateTaskRequest
            {
                TaskName = "Updated"
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateTask_ReturnsBadRequest_WhenStatusInvalid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PutAsJsonAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOpenId}", new UpdateTaskRequest
            {
                StatusId = 9999
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateTask_ReturnsOk_WhenValid()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PutAsJsonAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOpenId}", new UpdateTaskRequest
            {
                TaskName = "Updated Task",
                TaskDescription = "Updated description",
                StatusPriority = 5
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Updated Task", payload.GetProperty("task").GetProperty("taskName").GetString());
        }

        [Fact]
        public async Task DeleteTask_ReturnsUnauthorized_WhenNoSession()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.DeleteAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOpenId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_ReturnsNotFound_WhenTaskNotOwned()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.DeleteAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOtherUserId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_ReturnsOk_WhenOwned()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.DeleteAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOpenId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CloseTask_ReturnsUnauthorized_WhenNoSession()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PatchAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOpenId}/close", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CloseTask_ReturnsNotFound_WhenTaskNotOwned()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PatchAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOtherUserId}/close", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CloseTask_ReturnsOk_WhenOwned()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PatchAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOpenId}/close", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ReopenTask_ReturnsUnauthorized_WhenNoSession()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);

            var response = await client.PatchAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskClosedId}/reopen", null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ReopenTask_ReturnsNotFound_WhenTaskNotOwned()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PatchAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskOtherUserId}/reopen", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ReopenTask_ReturnsOk_WhenOwned()
        {
            await using var factory = new TaskFlowApiFactory();
            await factory.SeedDefaultDataAsync();

            using var client = factory.CreateClient(ClientOptions);
            client.DefaultRequestHeaders.Add("Cookie", $"TaskFlowRefreshToken={TestDataSeeder.ActiveRefreshToken}");

            var response = await client.PatchAsync($"/api/v1.0/tasks/{TestDataSeeder.TaskClosedId}/reopen", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
