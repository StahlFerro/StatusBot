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
        public DataAccess DA;
        public EventService ES;

        static Program()
        {
        }

        public async Task Start()
        {
            DA = new DataAccess();
            Console.WriteLine($"{DateTime.Now.ToLocalTime()} Starting StatusBot");
            File.AppendAllText("logfile.txt", $"{DateTime.Now.ToLocalTime()} Starting StatusBot\n");
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true
            });
            ES = new EventService(client);
            client.Log += ES.Log;
            var serviceprovider = ConfigureServices();
            string token = File.ReadAllText("token.txt");
            var time = Stopwatch.StartNew();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            time.Stop();
            Console.WriteLine($"{DateTime.Now.ToLocalTime()} Connected in {time.Elapsed.TotalSeconds.ToString("F3")} seconds");
            File.AppendAllText("logfile.txt", $"{DateTime.Now.ToLocalTime()} Connected in {time.Elapsed.TotalSeconds.ToString("F3")} seconds\n");

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
                .AddSingleton(new CommandService());
            var provider = services.BuildServiceProvider();
            return provider;
        }
    }
}
