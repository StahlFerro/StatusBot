using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StatusBot.Services.ReminderService;

namespace StatusBot
{
    class Program
    {
        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();
        public static DiscordSocketClient client;
        private CommandHandler handler;
        private ReminderService RS = new ReminderService();

        static Program()
        {
        }

        public async Task Start()
        {
            Console.WriteLine($"{DateTime.Now.ToLocalTime().ToLongTimeString()} Starting StatusBot");
            client = new DiscordSocketClient(new DiscordSocketConfig {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true
            });
            client.Log += Log;
            var serviceprovider = ConfigureServices();
            string tokenson = File.ReadAllText("token.json");
            string token = JsonConvert.DeserializeObject<string>(tokenson);
            var time = Stopwatch.StartNew();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            time.Stop();
            Console.WriteLine($"{DateTime.Now.ToLocalTime().ToLongTimeString()} Connected in " + time.Elapsed.TotalSeconds.ToString("F3") + " seconds");

            handler = new CommandHandler(serviceprovider);
            await handler.ConfigureAsync();

            client.MessageReceived += MessageReceived;
            client.Connected += AutoSetGame;
            client.GuildMemberUpdated += RS.AutoPM;
            client.JoinedGuild += RS.CreateNewRegistry;

            // Block this program until it is closed.
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage msg)
        {
            if (msg.Content.StartsWith("s]") || msg.Author.Id == 300611567874080769)
            {
                var ch = msg.Channel as IGuildChannel;
                var G = ch.Guild as IGuild;
                Console.WriteLine($"{msg.CreatedAt.LocalDateTime.ToLongTimeString()} [{G.Name}] ({msg.Channel}) {msg.Author}: {msg.Content}");
            }
            await Task.CompletedTask;
        }

        private Task Log(LogMessage msg) //For built-in Discord.Net logging feature
        {
            var cc = Console.ForegroundColor;
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    cc = ConsoleColor.Red; break;
                case LogSeverity.Warning:
                    cc = ConsoleColor.Yellow; break;
                case LogSeverity.Info:
                    cc = ConsoleColor.White; break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    cc = ConsoleColor.DarkGray; break;
            }
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(new CommandService());
            var provider = services.BuildServiceProvider();
            return provider;
        }

        private async Task AutoSetGame()
        {
            await client.SetGameAsync("type s]help");
        }
    }
}
