using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Discord;
using Discord.WebSocket;
using StatusBot.Services;

namespace StatusBot.Services
{
    public class EventService
    {
        DiscordSocketClient client;
        public EventService(DiscordSocketClient _client)
        {
            client = _client;
        }
        DataAccess DA = new DataAccess();

        public Task Log(LogMessage msg) //For built-in Discord.Net logging feature that logs to console and logfile
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

        public async Task MessageReceived(SocketMessage msg)
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

        public async Task AutoSetGame()
        {
            await client.SetGameAsync("type s]h");
        }

        public async Task AutoPM(SocketGuildUser before, SocketGuildUser after)
        {
            var G = before.Guild;
            var reminder = DA.GetReminderConfig(G, before);
            if (before.IsBot && reminder.Active && after.Status == UserStatus.Offline)
            {
                var listenerlist = DA.GetListenerList(G, before);
                if (!listenerlist.Any()) return;
                foreach (var listener in listenerlist)
                {
                    var U = Program.client.GetUser(Convert.ToUInt64(listener.UserID));
                    var dmch = await U.GetOrCreateDMChannelAsync();
                    await dmch.SendMessageAsync($"{after} is offline");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{DateTime.Now.ToLocalTime()} Successfully PM'd {U} that {after} is offline");
                    File.AppendAllText("logfile.txt", $"{DateTime.Now.ToLocalTime()} Successfully PM'd {U} that {after} is offline\n");
                    Console.ResetColor();
                }
            }
        }
    }
}
