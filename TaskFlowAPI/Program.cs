using BLFramework.Data;
using BLFramework.Services;
using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with XML documentation
builder.Services.AddSwaggerGen(options =>
{
    // Add XML documentation file
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Configure Swagger title and description
    options.SwaggerDoc("v1.0", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TaskFlow API",
        Version = "v1.0",
        Description = "REST API for the TaskFlow task management application",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "TaskFlow Team"
        }
    });
});

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
app.UseSwaggerUI(options =>
{
    // Make Swagger available at root
    options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "TaskFlow API v1.0");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
