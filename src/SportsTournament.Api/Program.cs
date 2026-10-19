using SportsTournament.Application.Common;
using SportsTournament.Application.Tournaments.Commands;
using SportsTournament.Application.Tournaments.Queries;
using SportsTournament.Infrastructure;
using SportsTournament.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<ITournamentListQuery, TournamentListQuery>();
builder.Services.AddScoped<ITournamentDetailsQuery, TournamentDetailsQuery>();
builder.Services.AddScoped<ICreateTournamentHandler, CreateTournamentHandler>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<ICurrentUserService, DevelopmentCurrentUserService>();
}

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
