using EMS.API.Middleware;
using EMS.Application;
using EMS.Application.DTOs.Common;
using EMS.Application.Validators;
using EMS.Infrastructure;
using EMS.Infrastructure.Data;
using EMS.Infrastructure.Data.Seed;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

// ── Bootstrap logger (captures startup errors) ────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("🚀 EMS API starting up...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext());

    // ── Controllers + Validation ──────────────────────────────────────────
    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors
                        .Select(x => x.ErrorMessage))
                    .ToList();

                var response = ApiResponseDto<object>.Fail(
                    "Validation failed.", errors);

                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
            };
        });

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

    builder.Services.AddEndpointsApiExplorer();

    // ── Swagger with JWT ──────────────────────────────────────────────────
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "EMS API",
            Version = "v1",
            Description = "Employee Management System API"
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization. Enter: Bearer {token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                        { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // ── Application & Infrastructure layers ───────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── CORS ──────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowReactApp", policy =>
            policy.WithOrigins(
                      "http://localhost:5173",
                      "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()));

    var app = builder.Build();

    // ── Exception middleware FIRST ─────────────────────────────────────────
    app.UseMiddleware<ExceptionMiddleware>();

    // ── Serilog request logging ────────────────────────────────────────────
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    });

    // ── DB migration + seed ────────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        Log.Information("Applying database migrations...");
        await db.Database.MigrateAsync();

        Log.Information("Seeding database...");
        await DatabaseSeeder.SeedAsync(db);
    }

    // ── Swagger (dev only) ─────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "EMS API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseStaticFiles();
    app.UseCors("AllowReactApp");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("✅ EMS API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ EMS API failed to start");
}
finally
{
    Log.CloseAndFlush();
}