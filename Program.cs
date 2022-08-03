using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

public class Program
{
    private DiscordSocketClient _client = null!;
    static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
        var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
        _client = new DiscordSocketClient(_config);

        var _commandService = new CommandService(new CommandServiceConfig
        {
            IgnoreExtraArgs = true,
            CaseSensitiveCommands = false,
            LogLevel = LogSeverity.Info
        });


        var logger = new LoggingService(_client, _commandService);

        var handler = new CommandHandler(_client, _commandService);
        await handler.InstallCommandsAsync();

        await _client.LoginAsync(TokenType.Bot, File.ReadAllText("./tokens/discord-token.txt"));
        await _client.StartAsync();

        _client.MessageUpdated += MessageUpdated;

        await RiotApiManager.UpdateContent();

        _client.Ready += () =>
        {
            System.Console.WriteLine("Bot Online");
            return Task.CompletedTask;
        };


        await Task.Delay(-1);
    }

    private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
    {
        // If the message was not in the cache, downloading it will result in getting a copy of `after`.
        var message = await before.GetOrDownloadAsync();
        Console.WriteLine($"{message} -> {after}");
    }
}
