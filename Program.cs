using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WeatherDashboardBackend.Models;
using WeatherDashboardBackend.Services;
using WeatherDashboardBackend.Data;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Load config including environment variables
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    // Get connection string from config or env var override
    string? connectionString = builder.Configuration.GetConnectionString("DefaultConnectionPostgreSQL");
    var envConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    if (!string.IsNullOrEmpty(envConnectionString))
    {
        connectionString = envConnectionString;
    }

    // Setup PostgreSQL DB Context
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));

    // Register services
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddSingleton<ITokenService, TokenService>();
    builder.Services.AddHttpClient<IWeatherService, WeatherService>();
    builder.Services.AddHttpClient<IWeatherNewsService, WeatherNewsService>();
    builder.Services.AddHttpClient<ITemperatureService, TemperatureService>();

    // Register JwtSettings
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    if (jwtSettings == null)
    {
        throw new InvalidOperationException("JWT settings are not configured.");
    }

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

    // Authorization
    builder.Services.AddAuthorization();

    // CORS (adjust URL accordingly)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp", policy =>
        {
            policy.WithOrigins("http://localhost:3000") // change for production frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // Health Checks service
    builder.Services.AddHealthChecks();

    // Replace AddControllers() with AddControllersWithViews() to enable MVC views & TempData support
    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    app.Urls.Add($"http://0.0.0.0:{port}");

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseCors("AllowReactApp");

    app.UseAuthentication();
    app.UseAuthorization();

    // Health checks middleware
    app.UseHealthChecks("/health");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    // Minimal logging to console to avoid further exceptions in ToString()
    Console.Error.WriteLine("Unhandled exception during app startup.");
    if (ex != null)
    {
        try
        {
            Console.Error.WriteLine($"Exception message: {ex.Message}");
            Console.Error.WriteLine($"Exception stack trace: {ex.StackTrace}");
        }
        catch
        {
            // Fallback if ToString or StackTrace throws
            Console.Error.WriteLine("Failed to read exception details.");
        }
    }
    else
    {
        Console.Error.WriteLine("Exception object was null.");
    }

    // Exit with error code
    Environment.Exit(1);
}
