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
        private DataService DS;
        private EventService ES;
        private LogService LS;
        private TimerService TS;
        private ReminderService RS;
        static Program()
        {
        }

        public async Task Start()
        {
            DS = new DataService();
            LS = new LogService();
            await LS.Write("Starting StatusBot", ConsoleColor.DarkGreen);
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });
            TS = new TimerService(client);
            RS = new ReminderService(client, TS, DS, LS);
            ES = new EventService(client, LS, DS, RS);
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
            client.GuildMemberUpdated += ES.OfflineListener;

            // Block this program until it is closed.
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(new CommandService())
                .AddSingleton(LS)
                .AddSingleton(DS)
                .AddSingleton(TS);
            var provider = services.BuildServiceProvider();
            return provider;
        }
    }
}
