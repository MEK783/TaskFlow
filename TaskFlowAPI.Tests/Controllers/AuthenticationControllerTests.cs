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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskFlowAPI.Controllers;
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

        #region Integration Tests

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

        #endregion

        #region Unit Tests

        [Fact]
        public async Task RegisterAsync_InvalidModelState_ReturnsBadRequest()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Username", "Required");

            var result = await controller.RegisterAsync(new RegisterRequest());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RegisterAsync_InvalidModelState_ReturnsBadRequestWithErrors()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("InviteCode", "Required");

            var result = await controller.RegisterAsync(new RegisterRequest());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            var message = GetAnonymousProperty<string>(badRequest.Value!, "message");
            Assert.Equal("Invalid input", message);
            var errors = GetAnonymousProperty<IEnumerable<object>>(badRequest.Value!, "errors");
            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task RegisterAsync_Success_ReturnsOk()
        {
            var databaseName = $"AuthController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            await SeedInviteAsync(controller, "INVITECODE123456", createdById: 1);

            var result = await controller.RegisterAsync(new RegisterRequest
            {
                Username = "new-user",
                Password = "abcdef",
                InviteCode = "INVITECODE123456"
            });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.User);
            Assert.Equal("new-user", response.User!.Username);
        }

        [Fact]
        public async Task LoginAsync_InvalidModelState_ReturnsBadRequest()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Username", "Required");

            var result = await controller.LoginAsync(new LoginRequest());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoginAsync_Success_RememberMeTrue_SetsCookie()
        {
            var databaseName = $"AuthController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            var passwordHash = PasswordHashingService.HashPassword("abcdef");
            await SeedUserAsync(controller, new User
            {
                Id = 1,
                Username = "login-user",
                Password = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                LastLogin = DateTime.UtcNow.AddDays(-1)
            });

            var result = await controller.LoginAsync(new LoginRequest
            {
                Username = "login-user",
                Password = "abcdef",
                RememberMe = true
            });

            Assert.IsType<OkObjectResult>(result);
            Assert.True(controller.Response.Headers.ContainsKey("Set-Cookie"));
        }

        [Fact]
        public async Task GenerateInviteAsync_InvalidModelState_ReturnsBadRequest()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("ExpirationDays", "Invalid");

            var result = await controller.GenerateInviteAsync(new GenerateInviteRequest());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GenerateInviteAsync_ExpirationClamped_ReturnsOk()
        {
            var databaseName = $"AuthController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            await SeedUserAndTokenAsync(controller, new User
            {
                Id = 2,
                Username = "invite-user",
                Password = "hashed",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                LastLogin = DateTime.UtcNow.AddDays(-1)
            }, "active-token");

            SetRefreshTokenCookie(controller, "active-token");
            var now = DateTime.UtcNow;

            var result = await controller.GenerateInviteAsync(new GenerateInviteRequest
            {
                ExpirationDays = 0
            });

            Assert.IsType<OkObjectResult>(result);

            using var scope = controller.HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var invite = await context.Invites.OrderByDescending(i => i.Id).FirstAsync();
            Assert.True(invite.ExpiresOn >= now.AddDays(1));
        }

        [Fact]
        public async Task RegisterAsync_NullRequest_ReturnsServerError()
        {
            var controller = CreateController();

            var result = await controller.RegisterAsync(null!);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_NullRequest_ReturnsServerError()
        {
            var controller = CreateController();

            var result = await controller.LoginAsync(null!);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task LogoutAsync_WhenLoggerThrows_ReturnsServerError()
        {
            var controller = CreateController(logger: new ThrowingInfoLogger<AuthenticationController>());

            var result = await controller.LogoutAsync();

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task LogoutAsync_InvalidToken_StillReturnsOk()
        {
            var controller = CreateController();
            SetRefreshTokenCookie(controller, "missing-token");

            var result = await controller.LogoutAsync();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GenerateInviteAsync_NoCookie_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = await controller.GenerateInviteAsync(new GenerateInviteRequest());

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GenerateInviteAsync_InvalidToken_ReturnsUnauthorized()
        {
            var databaseName = $"AuthController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);
            SetRefreshTokenCookie(controller, "missing-token");

            var result = await controller.GenerateInviteAsync(new GenerateInviteRequest());

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GenerateInviteAsync_UserNotFound_ReturnsUnauthorized()
        {
            var databaseName = $"AuthController-{Guid.NewGuid()}";
            TrackingNullUserService.Reset();
            var controller = CreateController(databaseName, useTrackingNullUserService: true);

            using (var scope = controller.HttpContext.RequestServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.EnsureCreatedAsync();
                context.RefreshTokens.Add(new RefreshToken
                {
                    Token = "valid-token",
                    UserId = 999,
                    ExpiresOn = DateTime.UtcNow.AddDays(1)
                });
                await context.SaveChangesAsync();
            }

            SetRefreshTokenCookie(controller, "valid-token");

            var result = await controller.GenerateInviteAsync(new GenerateInviteRequest());

            Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.True(TrackingNullUserService.Calls > 0);
        }

        [Fact]
        public async Task GenerateInviteAsync_AddAsyncThrows_ReturnsServerError()
        {
            var databaseName = $"AuthController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName, useThrowingInviteService: true);

            await SeedUserAndTokenAsync(controller, new User
            {
                Id = 5,
                Username = "invite-user",
                Password = "hashed",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                LastLogin = DateTime.UtcNow.AddDays(-1)
            }, "invite-token");

            SetRefreshTokenCookie(controller, "invite-token");

            var result = await controller.GenerateInviteAsync(new GenerateInviteRequest
            {
                ExpirationDays = 10
            });

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task RefreshTokenAsync_NoCookie_ReturnsUnauthorized()
        {
            var controller = CreateController();

            var result = await controller.RefreshTokenAsync();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RefreshTokenAsync_InvalidToken_ReturnsUnauthorized()
        {
            var databaseName = $"AuthController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            using (var scope = controller.HttpContext.RequestServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.EnsureCreatedAsync();
                context.RefreshTokens.Add(new RefreshToken
                {
                    Token = "expired-token",
                    UserId = 1,
                    ExpiresOn = DateTime.UtcNow.AddDays(-1)
                });
                await context.SaveChangesAsync();
            }

            SetRefreshTokenCookie(controller, "expired-token");

            var result = await controller.RefreshTokenAsync();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RefreshTokenAsync_UserMissing_ReturnsUnauthorized()
        {
            var databaseName = $"AuthController-{Guid.NewGuid()}";
            TrackingNullUserService.Reset();
            var controller = CreateController(databaseName, useTrackingNullUserService: true);

            using (var scope = controller.HttpContext.RequestServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.EnsureCreatedAsync();
                context.RefreshTokens.Add(new RefreshToken
                {
                    Token = "missing-user-token",
                    UserId = 123,
                    ExpiresOn = DateTime.UtcNow.AddDays(1)
                });
                await context.SaveChangesAsync();
            }

            SetRefreshTokenCookie(controller, "missing-user-token");

            var result = await controller.RefreshTokenAsync();

            Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.True(TrackingNullUserService.Calls > 0);
        }

        [Fact]
        public async Task RefreshTokenAsync_UserInactive_ReturnsUnauthorized()
        {
            var databaseName = $"AuthController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            using (var scope = controller.HttpContext.RequestServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.EnsureCreatedAsync();
                context.Users.Add(new User
                {
                    Id = 1,
                    Username = "inactive",
                    Password = "hashed",
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    LastLogin = DateTime.UtcNow.AddDays(-1)
                });
                context.RefreshTokens.Add(new RefreshToken
                {
                    Token = "active-token",
                    UserId = 1,
                    ExpiresOn = DateTime.UtcNow.AddDays(1)
                });
                await context.SaveChangesAsync();
            }

            SetRefreshTokenCookie(controller, "active-token");

            var result = await controller.RefreshTokenAsync();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RefreshTokenAsync_ServiceThrows_ReturnsServerError()
        {
            var databaseName = $"AuthController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName, useThrowingUserService: true);

            await SeedUserAndTokenAsync(controller, new User
            {
                Id = 4,
                Username = "throw-user",
                Password = "hashed",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                LastLogin = DateTime.UtcNow.AddDays(-1)
            }, "throw-token");

            SetRefreshTokenCookie(controller, "throw-token");

            var result = await controller.RefreshTokenAsync();

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task RefreshTokenAsync_Success_ReturnsOk()
        {
            var databaseName = $"AuthController-{Guid.NewGuid()}";
            var controller = CreateController(databaseName);

            await SeedUserAndTokenAsync(controller, new User
            {
                Id = 3,
                Username = "active-user",
                Password = "hashed",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                LastLogin = DateTime.UtcNow.AddDays(-1)
            }, "refresh-token");

            SetRefreshTokenCookie(controller, "refresh-token");

            var result = await controller.RefreshTokenAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.True(response.Success);
        }

        [Fact]
        public void MapUserToDto_MapsFields()
        {
            var controller = CreateController();
            var user = new User
            {
                Id = 10,
                Username = "user",
                IsActive = true,
                LastLogin = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var dto = InvokePrivate<UserDto>(controller, "MapUserToDto", user);

            Assert.Equal(user.Id, dto.Id);
            Assert.Equal(user.Username, dto.Username);
            Assert.Equal(user.IsActive, dto.IsActive);
            Assert.Equal(user.LastLogin, dto.LastLogin);
            Assert.Equal(user.CreatedAt, dto.CreatedAt);
        }

        [Fact]
        public void MapInviteToDto_MapsFields()
        {
            var controller = CreateController();
            var invite = new Invite
            {
                Id = 5,
                InviteCode = "CODE123456789012",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresOn = DateTime.UtcNow.AddDays(2),
                UsedOn = null
            };

            var dto = InvokePrivate<InviteDto>(controller, "MapInviteToDto", invite);

            Assert.Equal(invite.Id, dto.Id);
            Assert.Equal(invite.InviteCode, dto.InviteCode);
            Assert.Equal(invite.CreatedAt, dto.CreatedAt);
            Assert.Equal(invite.ExpiresOn, dto.ExpiresOn);
            Assert.Equal(invite.UsedOn, dto.UsedOn);
            Assert.Equal(invite.IsValid, dto.IsValid);
            Assert.Equal(invite.IsExpired, dto.IsExpired);
        }

        #endregion

        #region Helper Methods

        private static AuthenticationController CreateController(
            string? databaseName = null,
            ILogger<AuthenticationController>? logger = null,
            bool useThrowingInviteService = false,
            bool useThrowingUserService = false,
            bool useNullUserService = false,
            bool useTrackingNullUserService = false)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName ?? $"AuthController-{Guid.NewGuid()}"));

            if (useThrowingUserService)
            {
                services.AddScoped<UserService, ThrowingUserService>();
            }
            else if (useTrackingNullUserService)
            {
                services.AddScoped<UserService, TrackingNullUserService>();
            }
            else if (useNullUserService)
            {
                services.AddScoped<UserService, NullUserService>();
            }
            else
            {
                services.AddScoped<UserService>();
            }
            services.AddScoped<TaskStatusService>();
            services.AddScoped<TaskService>();
            if (useThrowingInviteService)
            {
                services.AddScoped<InviteService, ThrowingInviteService>();
            }
            else
            {
                services.AddScoped<InviteService>();
            }

            services.AddScoped<RefreshTokenService>();
            services.AddScoped<AuthenticationService>();
            services.AddScoped(typeof(ILogger<BaseService<User>>), sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<BaseService<User>>());
            services.AddScoped(typeof(ILogger<BaseService<Invite>>), sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<BaseService<Invite>>());
            services.AddScoped(typeof(ILogger<BaseService<RefreshToken>>), sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<BaseService<RefreshToken>>());
            services.AddScoped(typeof(ILogger<BaseService<UserTask>>), sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<BaseService<UserTask>>());
            services.AddScoped(typeof(ILogger<BaseService<TaskStatusDefinition>>), sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<BaseService<TaskStatusDefinition>>());

            var provider = services.BuildServiceProvider();
            var httpContext = new DefaultHttpContext
            {
                RequestServices = provider
            };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var resolvedLogger = logger ?? provider.GetRequiredService<ILogger<AuthenticationController>>();

            var controller = new AuthenticationController(
                provider.GetRequiredService<AuthenticationService>(),
                provider.GetRequiredService<UserService>(),
                provider.GetRequiredService<InviteService>(),
                provider.GetRequiredService<RefreshTokenService>(),
                resolvedLogger,
                configuration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

            return controller;
        }

        private static void SetRefreshTokenCookie(AuthenticationController controller, string token)
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

        private static TResult InvokePrivate<TResult>(AuthenticationController controller, string methodName, object arg)
        {
            var method = typeof(AuthenticationController)
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(method);
            return (TResult)method!.Invoke(controller, new[] { arg })!;
        }

        private static TProperty GetAnonymousProperty<TProperty>(object target, string propertyName)
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            Assert.NotNull(property);
            return (TProperty)property!.GetValue(target)!;
        }

        private static async Task SeedUserAsync(AuthenticationController controller, User user)
        {
            using var scope = controller.HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        private static async Task SeedInviteAsync(AuthenticationController controller, string inviteCode, int createdById)
        {
            using var scope = controller.HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();

            context.Users.Add(new User
            {
                Id = createdById,
                Username = "invite-creator",
                Password = "hashed",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                LastLogin = DateTime.UtcNow.AddDays(-1)
            });

            context.Invites.Add(new Invite
            {
                InviteCode = inviteCode,
                CreatedById = createdById,
                ExpiresOn = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });

            await context.SaveChangesAsync();
        }

        private static async Task SeedUserAndTokenAsync(AuthenticationController controller, User user, string token)
        {
            using var scope = controller.HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();

            context.Users.Add(user);
            context.RefreshTokens.Add(new RefreshToken
            {
                Token = token,
                UserId = user.Id,
                ExpiresOn = DateTime.UtcNow.AddDays(1)
            });

            await context.SaveChangesAsync();
        }

        #endregion

        #region Helper Classes

        private sealed class ThrowingInfoLogger<T> : ILogger<T>
        {
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => new NullScope();

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Information)
                {
                    throw new InvalidOperationException("Logging failed");
                }
            }

            private sealed class NullScope : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }

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

        private sealed class ThrowingInviteService : InviteService
        {
            public ThrowingInviteService(AppDbContext context, ILogger<BaseService<Invite>> logger)
                : base(context, logger)
            {
            }

            public override async Task<Invite> AddAsync(Invite entity)
            {
                await Task.Yield();
                throw new InvalidOperationException("Invite add failed");
            }
        }

        private sealed class ThrowingUserService : UserService
        {
            public ThrowingUserService(AppDbContext context, ILogger<BaseService<User>> logger)
                : base(context, logger)
            {
            }

            public override Task<User?> GetByIdAsync(int id)
            {
                throw new InvalidOperationException("User lookup failed");
            }
        }

        private sealed class NullUserService : UserService
        {
            public NullUserService(AppDbContext context, ILogger<BaseService<User>> logger)
                : base(context, logger)
            {
            }

            public override Task<User?> GetByIdAsync(int id)
            {
                return Task.FromResult<User?>(null);
            }
        }

        private sealed class TrackingNullUserService : UserService
        {
            public static int Calls { get; private set; }

            public TrackingNullUserService(AppDbContext context, ILogger<BaseService<User>> logger)
                : base(context, logger)
            {
            }

            public override Task<User?> GetByIdAsync(int id)
            {
                Calls++;
                return Task.FromResult<User?>(null);
            }

            public static void Reset()
            {
                Calls = 0;
            }
        }

        #endregion
    }
}
