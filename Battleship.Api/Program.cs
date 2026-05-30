using Battleship.Api.DTOs;
using Battleship.Api.Hosting;
using Battleship.Api.Mapping;
using Battleship.Core.AI;
using Battleship.Core.Engine;
using Battleship.Core.Placement;
using Battleship.Core.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ValidPlacements>();
builder.Services.AddSingleton<PlayerBoardValidator>();
builder.Services.AddSingleton<MoveProcessor>();
builder.Services.AddSingleton(sp => new BotPlacementService(sp.GetRequiredService<ValidPlacements>()));
builder.Services.AddSingleton(_ => new BotTargetingService());
builder.Services.AddSingleton<GameStore>();
builder.Services.AddSingleton<GameEngine>();

builder.Services.Configure<GameCleanupOptions>(
    builder.Configuration.GetSection(GameCleanupOptions.SectionName));
builder.Services.AddHostedService<GameCleanupService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("HealthCheck")
    .WithTags("System");

app.MapGet("/games", (GameEngine engine) =>
    {
        var summaries = engine.ListGames()
            .Select(GameStateMapper.ToSummary)
            .ToArray();

        return Results.Ok(summaries);
    })
    .WithName("ListGames")
    .WithTags("System")
    .Produces<GameSummaryResponse[]>(StatusCodes.Status200OK);

app.MapPost("/game", (CreateGameRequest request, GameEngine engine) =>
    {
        var result = engine.CreateGame(request.PlayerBoard, request.BotMovesFirst);

        return result.IsSuccess
            ? Results.Created($"/game/{result.Game!.GameId}", GameStateMapper.ToResponse(result.Game))
            : ApiErrorMapper.ToResult(result.ErrorCode!, result.ErrorMessage!);
    })
    .WithName("CreateGame")
    .WithTags("Game")
    .Produces<GameStateResponse>(StatusCodes.Status201Created)
    .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

app.MapPost("/game/{gameId:guid}/moves", (Guid gameId, MoveRequest request, GameEngine engine) =>
    {
        var result = engine.SubmitMove(gameId, request.Location);

        return result.IsSuccess
            ? Results.Ok(GameStateMapper.ToResponse(result.Game!))
            : ApiErrorMapper.ToResult(result.ErrorCode!, result.ErrorMessage!);
    })
    .WithName("SubmitMove")
    .WithTags("Game")
    .Produces<GameStateResponse>(StatusCodes.Status200OK)
    .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
    .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
    .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

app.MapGet("/game/{gameId:guid}", (Guid gameId, GameEngine engine) =>
    {
        var result = engine.GetGame(gameId);

        return result.IsSuccess
            ? Results.Ok(GameStateMapper.ToResponse(result.Game!))
            : ApiErrorMapper.ToResult(result.ErrorCode!, result.ErrorMessage!);
    })
    .WithName("GetGame")
    .WithTags("Game")
    .Produces<GameStateResponse>(StatusCodes.Status200OK)
    .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

app.MapDelete("/game/{gameId:guid}", (Guid gameId, GameEngine engine) =>
    {
        return engine.DeleteGame(gameId)
            ? Results.Ok(new DeleteGameResponse { Success = true })
            : ApiErrorMapper.ToResult(
                ValidationErrorCode.GameNotFound,
                $"Game {gameId} was not found.");
    })
    .WithName("DeleteGame")
    .WithTags("Game")
    .Produces<DeleteGameResponse>(StatusCodes.Status200OK)
    .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

app.Run();

public partial class Program;
