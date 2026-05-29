using EMS.Application;
using EMS.Infrastructure;
using EMS.Infrastructure.Data;
using EMS.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Auto migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    Console.WriteLine("Applying migrations...");
    await db.Database.MigrateAsync();

    Console.WriteLine("Seeding database...");
    await DatabaseSeeder.SeedAsync(db);
}

app.UseAuthorization();

app.MapControllers();

app.Run();