using SportsTournament.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SportsTournament.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet(
    "/health/database",
    async (PlatformDbContext db, CancellationToken cancellationToken) =>
    {
        var canConnect = await db.Database.CanConnectAsync(cancellationToken);

        return canConnect ? Results.Ok(new
            {
                status = "Healthy",
                database = "Connected"
            })
            : Results.Problem(title: "Database is unavailable", statusCode: StatusCodes.Status503ServiceUnavailable);
    });

app.Run();
