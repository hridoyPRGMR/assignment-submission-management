using System.Text;
using System.Text.Json.Serialization;
using AssignmentSystem.Api.Authorization;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application;
using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Infrastructure;
using AssignmentSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<AssignmentSystem.Api.Middleware.GlobalExceptionHandler>();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? string.Empty;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddSingleton<IAuthorizationHandler, AssignmentOwnershipAuthorizationHandler>();

    builder.Services.AddAuthorization(options =>
    {
        options.AddAppAuthorizationPolicies();
        options.AddPolicy(AuthorizationPolicies.AssignmentOwner, policy =>
        {
            policy.RequireRole(AssignmentSystem.Domain.Enums.UserRole.Teacher.ToString());
            policy.Requirements.Add(new AssignmentOwnershipRequirement());
        });
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Assignment & Submission Management System API",
            Version = "v1"
        });

        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            Scheme = "bearer",
            BearerFormat = "JWT",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Description = "Enter a valid JWT access token"
        };

        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);
        options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, null),
                new List<string>()
            }
        });
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter("login", limiterOptions =>
        {
            limiterOptions.PermitLimit = builder.Configuration.GetValue("RateLimiting:Login:PermitLimit", 5);
            limiterOptions.Window = TimeSpan.FromSeconds(
                builder.Configuration.GetValue("RateLimiting:Login:WindowSeconds", 60));
            limiterOptions.QueueLimit = 0;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendPolicy", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000" };

            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    var app = builder.Build();

    // Defaults preserve the original dev-only behavior; a hosting platform (e.g. Render) can opt
    // in via Database__ApplyMigrationsOnStartup / Database__SeedDemoData env vars so a freshly
    // provisioned database (e.g. Neon) is ready without a manual migration step, and so the
    // required demo accounts exist on the live deployment.
    var applyMigrationsOnStartup = builder.Configuration.GetValue(
        "Database:ApplyMigrationsOnStartup", app.Environment.IsDevelopment());
    var seedDemoData = builder.Configuration.GetValue(
        "Database:SeedDemoData", app.Environment.IsDevelopment());

    if (applyMigrationsOnStartup || seedDemoData)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (applyMigrationsOnStartup)
        {
            await dbContext.Database.MigrateAsync();
        }

        if (seedDemoData)
        {
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await DataSeeder.SeedAsync(dbContext, passwordHasher);
        }
    }

    // Render (and most PaaS hosts) terminate TLS at a reverse proxy and forward plain HTTP,
    // signalling the original scheme via X-Forwarded-*. Without this, UseHttpsRedirection() and
    // any Request.IsHttps checks would see "http" and loop or misbehave. KnownNetworks/KnownProxies
    // are cleared because the proxy's IP isn't fixed/known ahead of time on these platforms.
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        KnownNetworks = { },
        KnownProxies = { }
    });

    app.UseSerilogRequestLogging();

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Assignment System API v1");
        });
    }

    app.UseHttpsRedirection();

    app.UseCors("FrontendPolicy");

    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Console.WriteLine("Application started. Press Ctrl+C to shut down.");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
