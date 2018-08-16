using System;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using StatusBot.Services;

namespace StatusBot
{
    class Program
    {
        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();
        public static DiscordSocketClient client;
        private CommandHandler handler;
        private DataAccess DA;
        private EventService ES;
        private LogService LS;
        private Formatter F;
        static Program()
        {
        }

        public async Task Start()
        {
            F = new Formatter();
            DA = new DataAccess(F);
            LS = new LogService();
            await LS.Write("Starting StatusBot", ConsoleColor.DarkGreen);
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });
            ES = new EventService(client, LS, DA);
            client.Log += ES.Log;
            var serviceprovider = ConfigureServices();
            string token = File.ReadAllText("token.txt");
            var time = Stopwatch.StartNew();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            time.Stop();
            await LS.Write($"Connected in {time.Elapsed.TotalSeconds.ToString("F3")} seconds", ConsoleColor.DarkGreen);

            handler = new CommandHandler(serviceprovider);
            await handler.ConfigureAsync();

            client.MessageReceived += ES.MessageReceived;
            client.Connected += ES.AutoSetGame;
            client.GuildMemberUpdated += ES.AutoPM;

            // Block this program until it is closed.
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(new CommandService())
                .AddSingleton(LS)
                .AddSingleton(F)
                .AddSingleton(DA);
            var provider = services.BuildServiceProvider();
            return provider;
        }
    }
}
