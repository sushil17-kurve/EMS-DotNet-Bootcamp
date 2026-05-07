using EMS.Infrastructure;
using EMS.Infrastructure.Data;
using EMS.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    Console.WriteLine("Applying migrations...");
    await db.Database.MigrateAsync();

    Console.WriteLine("Seeding database...");
    await DatabaseSeeder.SeedAsync(db);
}

app.Run();