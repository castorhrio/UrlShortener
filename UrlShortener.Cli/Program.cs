using System.CommandLine;
using StackExchange.Redis;
using UrlShortener.Data;

string URL_SHORTENER_CONNECTION_STRING = "127.0.0.1:6379";

var destinationOption = new Option<string>(new[] { "--destination-url", "-d" }, description: "The URL that shortened URL will forward to.");

destinationOption.IsRequired = true;
destinationOption.AddValidator(result =>
{
    var destination = result.Tokens[0].Value;
    if (ShortUrlValidator.ValidateDestination(destination, out var validationResults) == false)
    {
        result.ErrorMessage = string.Join(",", validationResults);
    }
});

var pathOption = new Option<string>(
    new[] { "--path", "-p" },
    description: "The path used for the shortened URL");

pathOption.IsRequired = true;
pathOption.AddValidator(result =>
{
    var path = result.Tokens[0].Value;
    if (ShortUrlValidator.ValidatePath(path, out var validationResults) == false)
    {
        result.ErrorMessage = string.Join(",", validationResults);
    }
});

var connectionStringOption = new Option<string?>(new[] { "--connection-string", "-c" }, description: "Connection string to connect to the Redis Database wherre Urls are stored" + "Alternativaly,you can set the 'URL_SHORTENER_CONNECTION_STRING'");

var envConnectionString = URL_SHORTENER_CONNECTION_STRING;
if (string.IsNullOrEmpty(envConnectionString))
{
    connectionStringOption.IsRequired = true;
}

var rootCommand = new RootCommand("Message the shortened URLs");
async Task<ConnectionMultiplexer> GetRedisConnection(string? connectionString)
{
    var redisConnection = await ConnectionMultiplexer.ConnectAsync(
        connectionString ??
        envConnectionString ??
        throw new System.Exception("Missing connection string")
    );

    return redisConnection;
}


var createCommand = new Command("create", "Create a shortened URL"){
    destinationOption,
    pathOption,
    connectionStringOption
};

createCommand.SetHandler(async (destination, path, connectionString) =>
{
    var shortUrlResposiroty = new ShortUrlRespository(await GetRedisConnection(connectionString));

    try
    {
        await shortUrlResposiroty.Create(new ShortUrl(destination, path));
        Console.WriteLine("Shortened URL create.");
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
    }
}, destinationOption, pathOption, connectionStringOption);

rootCommand.AddCommand(createCommand);

var updateCommand = new Command("update", "Update a shortened URL"){
    destinationOption,
    pathOption,
    connectionStringOption
};

updateCommand.SetHandler(async (destination, path, connectionString) =>
{
    var shortUrlRepository = new ShortUrlRespository(await GetRedisConnection(connectionString));
    try
    {
        await shortUrlRepository.Update(new ShortUrl(destination, path));
        Console.WriteLine("shortened URL update.");
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
    }
}, destinationOption, pathOption, connectionStringOption);

rootCommand.AddCommand(updateCommand);

var deleteCommand = new Command("delete", "Delete a shortened URL")
{
    pathOption,
    connectionStringOption
};

deleteCommand.SetHandler(async (path, connectionString) =>
{
    var shortUrlRespository = new ShortUrlRespository(await GetRedisConnection(connectionString));
    try
    {
        await shortUrlRespository.Delete(path);
        Console.WriteLine("Shortened URL deleted");
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
}, pathOption, connectionStringOption);

rootCommand.AddCommand(deleteCommand);

var getCommand = new Command("get", "Get a shortened URL")
{
    pathOption,
    connectionStringOption
};

getCommand.SetHandler(async (path, connectionString) =>
{
    var shortUrlResposiroty = new ShortUrlRespository(await GetRedisConnection(connectionString));
    try
    {
        var shortUrl = await shortUrlResposiroty.Get(path);
        if (shortUrl == null)
            Console.Error.WriteLine($"Shortened URL for path '{path}' not found.");
        else
            Console.WriteLine($"Destination URL:{shortUrl.Destination}, Path:{path}");
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
    }
}, pathOption, connectionStringOption);

rootCommand.AddCommand(getCommand);

var listCommand = new Command("list", "List shortened URL")
{
    connectionStringOption
};

listCommand.SetHandler(async (connectionString) =>
{
    var shortUrlRepository = new ShortUrlRespository(await GetRedisConnection(connectionString));
    try
    {
        var shortUrls = await shortUrlRepository.GetAll();
        foreach (var shortUrl in shortUrls)
        {
            Console.WriteLine($"Destination URL:{shortUrl.Destination}, Path:{shortUrl.Path}");
        }
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
    }
}, connectionStringOption);

rootCommand.AddCommand(listCommand);

return rootCommand.InvokeAsync(args).Result;
