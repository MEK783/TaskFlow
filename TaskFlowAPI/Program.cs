using BLFramework.Data;
using BLFramework.Services;
using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register BL Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TaskStatusService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<InviteService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<AuthenticationService>();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure closed task cleanup service
builder.Services.Configure<ClosedTaskCleanupOptions>(
    builder.Configuration.GetSection("ClosedTaskCleanup"));
builder.Services.AddHostedService<ClosedTaskCleanupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
