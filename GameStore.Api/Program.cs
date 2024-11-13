using GameStore.Api.Data;
using GameStore.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

//1. Connect to the DB
var DbConnString = builder.Configuration.GetConnectionString("GameStoreDb");
builder.Services.AddSqlite<GameStoreContext>(DbConnString);

//2.Initialize the app
var app = builder.Build();

//3. Map the endpoints
app.MapGamesEndpoints();
app.MapGenresEndpoints();

//4. Apply DB Migrations
await app.MigrateDbAsync();

//5. Start the app server
app.Run();