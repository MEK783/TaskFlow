using System.Security.Cryptography;
using System.Text;
using BLFramework.Data;
using BLFramework.Models;
using BLFramework.Services;

namespace TaskFlowAPI.Tests.Fixtures
{
    internal static class TestDataSeeder
    {
        public const int UserAliceId = 1;
        public const int UserBobId = 2;
        public const int UserInactiveId = 3;

        public const int StatusTodoId = 1;
        public const int StatusDoneId = 2;

        public const string DefaultPassword = "Password123!";
        public static readonly string DefaultPasswordSha512 = ComputeSha512(DefaultPassword);

        public const string ValidInviteCode = "VALIDINVITE12345";
        public const string ExpiredInviteCode = "EXPIREDINVITE000";
        public const string UsedInviteCode = "USEDINVITE000000";

        public const string ActiveRefreshToken = "active-refresh-token-0001";
        public const string ExpiredRefreshToken = "expired-refresh-token-0001";
        public const string RevokedRefreshToken = "revoked-refresh-token-0001";

        public const int TaskOpenId = 1001;
        public const int TaskClosedId = 1002;
        public const int TaskOtherUserId = 2001;

        public static async Task ResetAsync(AppDbContext context)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }

        public static async Task SeedAsync(AppDbContext context)
        {
            var todoStatus = new TaskStatusDefinition
            {
                Id = StatusTodoId,
                StatusCode = "TODO",
                StatusDescription = "To do",
                ReactIcon = "ai/todo",
                ClosingStatus = false
            };

            var doneStatus = new TaskStatusDefinition
            {
                Id = StatusDoneId,
                StatusCode = "DONE",
                StatusDescription = "Done",
                ReactIcon = "ai/checkCircle",
                ClosingStatus = true
            };

            var alice = new User
            {
                Id = UserAliceId,
                Username = "alice",
                Password = PasswordHashingService.HashPassword(DefaultPasswordSha512),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                LastLogin = DateTime.UtcNow.AddDays(-1)
            };

            var bob = new User
            {
                Id = UserBobId,
                Username = "bob",
                Password = PasswordHashingService.HashPassword(DefaultPasswordSha512),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                LastLogin = DateTime.UtcNow.AddDays(-2)
            };

            var inactive = new User
            {
                Id = UserInactiveId,
                Username = "inactive",
                Password = PasswordHashingService.HashPassword(DefaultPasswordSha512),
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                LastLogin = DateTime.UtcNow.AddDays(-5)
            };

            var validInvite = new Invite
            {
                Id = 10,
                InviteCode = ValidInviteCode,
                CreatedById = UserAliceId,
                ExpiresOn = DateTime.UtcNow.AddDays(10),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var expiredInvite = new Invite
            {
                Id = 11,
                InviteCode = ExpiredInviteCode,
                CreatedById = UserAliceId,
                ExpiresOn = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            };

            var usedInvite = new Invite
            {
                Id = 12,
                InviteCode = UsedInviteCode,
                CreatedById = UserAliceId,
                ExpiresOn = DateTime.UtcNow.AddDays(10),
                UsedOn = DateTime.UtcNow.AddDays(-1),
                UsedById = UserBobId,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            var taskOpen = new UserTask
            {
                Id = TaskOpenId,
                TaskName = "Task One",
                TaskDescription = "Open task",
                StatusId = StatusTodoId,
                CreatedById = UserAliceId,
                StatusPriority = 0,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ModifiedOn = DateTime.UtcNow.AddDays(-1)
            };

            var taskClosed = new UserTask
            {
                Id = TaskClosedId,
                TaskName = "Task Two",
                TaskDescription = "Closed task",
                StatusId = StatusDoneId,
                CreatedById = UserAliceId,
                StatusPriority = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ModifiedOn = DateTime.UtcNow.AddDays(-2),
                ClosedOn = DateTime.UtcNow.AddDays(-1)
            };

            var taskOtherUser = new UserTask
            {
                Id = TaskOtherUserId,
                TaskName = "Bob Task",
                TaskDescription = "Other user task",
                StatusId = StatusTodoId,
                CreatedById = UserBobId,
                StatusPriority = 0,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ModifiedOn = DateTime.UtcNow.AddDays(-1)
            };

            var activeToken = new RefreshToken
            {
                Id = 100,
                Token = ActiveRefreshToken,
                UserId = UserAliceId,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var expiredToken = new RefreshToken
            {
                Id = 101,
                Token = ExpiredRefreshToken,
                UserId = UserAliceId,
                ExpiresOn = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            var revokedToken = new RefreshToken
            {
                Id = 102,
                Token = RevokedRefreshToken,
                UserId = UserAliceId,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                RevokedOn = DateTime.UtcNow.AddHours(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            context.TaskStatusDefinitions.AddRange(todoStatus, doneStatus);
            context.Users.AddRange(alice, bob, inactive);
            context.Invites.AddRange(validInvite, expiredInvite, usedInvite);
            context.Tasks.AddRange(taskOpen, taskClosed, taskOtherUser);
            context.RefreshTokens.AddRange(activeToken, expiredToken, revokedToken);

            await context.SaveChangesAsync();
        }

        public static string ComputeSha512(string input)
        {
            using var sha = SHA512.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
