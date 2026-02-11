using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using BLFramework.Data;
using BLFramework.Models;
using BLFramework.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskFlowAPI.Controllers;
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

        #region Integration Tests

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

        #endregion

        #region Unit Tests

        [Fact]
        public async Task GetAllTasksAsync_InactiveToken_ReturnsUnauthorized()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            using (var scope = controller.HttpContext.RequestServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.EnsureCreatedAsync();
                context.RefreshTokens.Add(new RefreshToken
                {
                    Token = "inactive-token",
                    UserId = 1,
                    ExpiresOn = DateTime.UtcNow.AddDays(-1) // Expired
                });
                await context.SaveChangesAsync();
            }

            SetRefreshTokenCookie(controller, "inactive-token");

            var result = await controller.GetAllTasksAsync();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetAllTasksAsync_TokenNotFound_ReturnsUnauthorized()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            SetRefreshTokenCookie(controller, "nonexistent-token");

            var result = await controller.GetAllTasksAsync();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetTaskByIdAsync_TaskNotFound_ReturnsNotFound()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.GetTaskByIdAsync(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetTasksByStatusAsync_InactiveToken_ReturnsUnauthorized()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            using (var scope = controller.HttpContext.RequestServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.EnsureCreatedAsync();
                context.RefreshTokens.Add(new RefreshToken
                {
                    Token = "revoked-token",
                    UserId = 1,
                    ExpiresOn = DateTime.UtcNow.AddDays(1),
                    RevokedOn = DateTime.UtcNow.AddHours(-1) // Revoked
                });
                await context.SaveChangesAsync();
            }

            SetRefreshTokenCookie(controller, "revoked-token");

            var result = await controller.GetTasksByStatusAsync(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task CreateTaskAsync_InvalidModelState_ReturnsBadRequest()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("TaskName", "Required");

            var result = await controller.CreateTaskAsync(new CreateTaskRequest());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateTaskAsync_InactiveToken_ReturnsUnauthorized()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            using (var scope = controller.HttpContext.RequestServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.EnsureCreatedAsync();
                context.RefreshTokens.Add(new RefreshToken
                {
                    Token = "old-token",
                    UserId = 1,
                    ExpiresOn = DateTime.UtcNow.AddDays(-2) // Expired
                });
                await context.SaveChangesAsync();
            }

            SetRefreshTokenCookie(controller, "old-token");

            var result = await controller.CreateTaskAsync(new CreateTaskRequest
            {
                TaskName = "New Task",
                StatusId = 1
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task CreateTaskAsync_ServiceThrows_ReturnsServerError()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName, useThrowingTaskService: true);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            await SeedTaskStatusAsync(controller, 1);
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.CreateTaskAsync(new CreateTaskRequest
            {
                TaskName = "Test Task",
                TaskDescription = "Description",
                StatusId = 1
            });

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateTaskAsync_NoCookie_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = await controller.UpdateTaskAsync(1, new UpdateTaskRequest());

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UpdateTaskAsync_StatusIdChangeToInvalid_ReturnsBadRequest()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            await SeedTaskStatusAsync(controller, 1);
            var taskId = await SeedTaskAsync(controller, 1, 1, "Test Task");
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.UpdateTaskAsync(taskId, new UpdateTaskRequest
            {
                StatusId = 9999
            });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateTaskAsync_OnlyPriorityChange_ReturnsOk()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            await SeedTaskStatusAsync(controller, 1);
            var taskId = await SeedTaskAsync(controller, 1, 1, "Test Task");
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.UpdateTaskAsync(taskId, new UpdateTaskRequest
            {
                StatusPriority = 10
            });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateTaskAsync_DescriptionNull_ReturnsOk()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            await SeedTaskStatusAsync(controller, 1);
            var taskId = await SeedTaskAsync(controller, 1, 1, "Test Task");
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.UpdateTaskAsync(taskId, new UpdateTaskRequest
            {
                TaskDescription = null
            });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateTaskAsync_ServiceThrows_ReturnsServerError()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName, useThrowingTaskService: true);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.UpdateTaskAsync(1, new UpdateTaskRequest
            {
                TaskName = "Updated"
            });

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteTaskAsync_TaskNotFound_ReturnsNotFound()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.DeleteTaskAsync(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteTaskAsync_ServiceThrows_ReturnsServerError()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName, useThrowingTaskService: true);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.DeleteTaskAsync(1);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task CloseTaskAsync_TaskNotFound_ReturnsNotFound()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.CloseTaskAsync(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CloseTaskAsync_ServiceThrows_ReturnsServerError()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName, useThrowingTaskService: true);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.CloseTaskAsync(1);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task ReopenTaskAsync_TaskNotFound_ReturnsNotFound()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.ReopenTaskAsync(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ReopenTaskAsync_ServiceThrows_ReturnsServerError()
        {
            var databaseName = $"TasksController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName, useThrowingTaskService: true);

            await SeedUserAndTokenAsync(controller, 1, "test-token");
            SetRefreshTokenCookie(controller, "test-token");

            var result = await controller.ReopenTaskAsync(1);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        #endregion

        #region Helper Methods

        private static TasksController CreateController(
            string? databaseName = null,
            bool useThrowingTaskService = false)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName ?? $"TasksController-{Guid.NewGuid()}"));

            if (useThrowingTaskService)
            {
                services.AddScoped<TaskService, ThrowingTaskService>();
            }
            else
            {
                services.AddScoped<TaskService>();
            }

            services.AddScoped<TaskStatusService>();
            services.AddScoped<RefreshTokenService>();
            services.AddScoped<UserService>();
            services.AddScoped(typeof(ILogger<BaseService<UserTask>>), sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<BaseService<UserTask>>());
            services.AddScoped(typeof(ILogger<BaseService<TaskStatusDefinition>>), sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<BaseService<TaskStatusDefinition>>());
            services.AddScoped(typeof(ILogger<BaseService<RefreshToken>>), sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<BaseService<RefreshToken>>());
            services.AddScoped(typeof(ILogger<BaseService<User>>), sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<BaseService<User>>());

            var provider = services.BuildServiceProvider();
            var httpContext = new DefaultHttpContext
            {
                RequestServices = provider
            };

            var logger = provider.GetRequiredService<ILogger<TasksController>>();

            var controller = new TasksController(
                provider.GetRequiredService<TaskService>(),
                provider.GetRequiredService<TaskStatusService>(),
                provider.GetRequiredService<RefreshTokenService>(),
                provider.GetRequiredService<UserService>(),
                logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

            return controller;
        }

        private static void SetRefreshTokenCookie(TasksController controller, string token)
        {
            var cookies = new Dictionary<string, string>
            {
                ["TaskFlowRefreshToken"] = token
            };

            var newContext = new DefaultHttpContext
            {
                RequestServices = controller.HttpContext.RequestServices
            };

            newContext.Request.Headers["Cookie"] = $"TaskFlowRefreshToken={token}";
            newContext.Features.Set<IRequestCookiesFeature>(
                new RequestCookiesFeature(new TestCookieCollection(cookies)));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = newContext
            };
        }

        private static async Task SeedUserAndTokenAsync(TasksController controller, int userId, string token)
        {
            using var scope = controller.HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();

            context.Users.Add(new User
            {
                Id = userId,
                Username = $"user{userId}",
                Password = "hashed",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                LastLogin = DateTime.UtcNow.AddHours(-1)
            });

            context.RefreshTokens.Add(new RefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresOn = DateTime.UtcNow.AddDays(1)
            });

            await context.SaveChangesAsync();
        }

        private static async Task SeedTaskStatusAsync(TasksController controller, int statusId)
        {
            using var scope = controller.HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();

            context.TaskStatusDefinitions.Add(new TaskStatusDefinition
            {
                Id = statusId,
                StatusCode = "TODO",
                StatusDescription = "To Do",
                ReactIcon = "icon",
                ClosingStatus = false,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            });

            await context.SaveChangesAsync();
        }

        private static async Task<int> SeedTaskAsync(TasksController controller, int userId, int statusId, string taskName)
        {
            using var scope = controller.HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();

            var task = new UserTask
            {
                TaskName = taskName,
                TaskDescription = "Description",
                StatusId = statusId,
                CreatedById = userId,
                StatusPriority = 0,
                CreatedAt = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow
            };

            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            return task.Id;
        }

        #endregion

        #region Helper Classes

        private sealed class TestCookieCollection : Dictionary<string, string>, IRequestCookieCollection
        {
            public TestCookieCollection(IDictionary<string, string> values)
                : base(values, StringComparer.OrdinalIgnoreCase)
            {
            }

            string IRequestCookieCollection.this[string key] => TryGetValue(key, out var value) ? value : string.Empty;

            public new int Count => base.Count;

            public new ICollection<string> Keys => base.Keys;

            public new bool TryGetValue(string key, out string value) => base.TryGetValue(key, out value!);
        }

        private sealed class ThrowingTaskService : TaskService
        {
            public ThrowingTaskService(AppDbContext context, ILogger<BaseService<UserTask>> logger)
                : base(context, logger)
            {
            }

            public override Task<UserTask?> GetByIdAsync(int id)
            {
                throw new InvalidOperationException("Service error");
            }

            public override Task<UserTask> AddAsync(UserTask entity)
            {
                throw new InvalidOperationException("Service error");
            }

            public override Task<UserTask> UpdateAsync(UserTask entity)
            {
                throw new InvalidOperationException("Service error");
            }

            public override Task DeleteAsync(UserTask entity)
            {
                throw new InvalidOperationException("Service error");
            }
        }

        #endregion
    }
}
