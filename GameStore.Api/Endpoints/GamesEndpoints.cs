using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
  const string GetGameEndpointName = "GetGame";

  public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("games").WithParameterValidation();

    //1. GET /games
    group.MapGet("/", async (GameStoreContext dbContext) =>
      await dbContext.Games
        .Include(game => game.Genre)
        .Select(game => game.ToGameSummaryDto())
        .AsNoTracking()
        .ToListAsync()
    );

    //2. GET /games/:gameId
    group.MapGet("/{id}", async (int id, GameStoreContext dbContext) =>
    {
      Game? game = await dbContext.Games.FindAsync(id);
      return game is null ? Results.NotFound() : Results.Ok(game.ToGameDetailsDto());
    }).WithName(GetGameEndpointName);

    //3. POST /games
    group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) =>
    {
      Game game = newGame.ToGameEntity();

      //Save the new game in the DB
      dbContext.Games.Add(game);
      await dbContext.SaveChangesAsync();

      return Results.CreatedAtRoute(
        GetGameEndpointName,
        new { id = game.Id },
        game.ToGameDetailsDto()
      );
    });

    //4. PUT /games/:gameId
    group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
    {
      var existingGame = await dbContext.Games.FindAsync(id);

      if (existingGame is null)
      {
        return Results.NotFound();
      }

      dbContext.Entry(existingGame)
        .CurrentValues.SetValues(updatedGame.ToGameEntity(id));

      await dbContext.SaveChangesAsync();

      return Results.Ok(updatedGame);
    });

    //5. DELETE /games/:gameId
    group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
    {
      await dbContext.Games
        .Where(game => game.Id == id)
        .ExecuteDeleteAsync();

      return Results.NoContent();
    });

    return group;
  }
};