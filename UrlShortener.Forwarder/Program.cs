
using StackExchange.Redis;
using UrlShortener.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "127.0.0.1:6379";

var redisConnection = await ConnectionMultiplexer.ConnectAsync(connectionString);
builder.Services.AddSingleton(redisConnection);
builder.Services.AddTransient<ShortUrlRespository>();

var app = builder.Build();

app.MapGet("/{path}", async (string path, ShortUrlRespository shortUrlRespository) =>
{
    if (ShortUrlValidator.ValidatePath(path, out _))
        return Results.BadRequest();

    var shortUrl = await shortUrlRespository.Get(path);
    if (shortUrl == null || string.IsNullOrEmpty(shortUrl.Destination))
        return Results.NotFound();

    return Results.Redirect(shortUrl.Destination);
});

app.Run();

