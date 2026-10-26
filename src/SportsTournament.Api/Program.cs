using SportsTournament.Api.BackgroundServices;
using SportsTournament.Api.Configuration;
using SportsTournament.Api.ExceptionHandling;
using SportsTournament.Application;
using SportsTournament.Application.Common;
using SportsTournament.Infrastructure;
using SportsTournament.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationSettings(builder.Configuration);
builder.Services.AddHostedService<OutboxBackgroundService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<ICurrentUserService, DevelopmentCurrentUserService>();
}

builder.Services.AddControllers(options =>
{
    options.ValidateComplexTypesIfChildValidationFails = true;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

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
