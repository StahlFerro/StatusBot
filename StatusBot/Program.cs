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
            client.Log += Log;
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

            client.MessageReceived += MessageReceived;
            client.Connected += AutoSetGame;
            client.GuildMemberUpdated += DA.AutoPM;

            // Block this program until it is closed.
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage msg)
        {
            //Outputs to console and logs to logfile if the message starts with the s] prefix or is from StatusBot itself
            if (msg.Content.StartsWith("s]") || msg.Author.Id == 332603467577425929)
            {
                var ch = msg.Channel as IGuildChannel;
                var G = ch.Guild as IGuild;
                Console.WriteLine($"{msg.CreatedAt.LocalDateTime} [{G.Name}] ({msg.Channel}) {msg.Author}: {msg.Content}");
                File.AppendAllText("logfile.txt", $"{msg.CreatedAt.LocalDateTime} [{G.Name}] ({msg.Channel}) {msg.Author}: {msg.Content}\n");
            }
            await Task.CompletedTask;
        }

        private Task Log(LogMessage msg) //For built-in Discord.Net logging feature that logs to console and logfile
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
            Console.WriteLine($"{DateTime.Now.ToShortDateString()} {msg.ToString()}");
            File.AppendAllText("logfile.txt", $"{DateTime.Now.ToShortDateString()} {msg.ToString()}\n");
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
            await client.SetGameAsync("type s]h");
        }
    }
}
