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
        DiscordSocketClient _client;
        LogService _logservice;
        DataAccess DA;
        public EventService(DiscordSocketClient client, LogService logservice, DataAccess dataAccess)
        {
            _client = client;
            _logservice = logservice;
            DA = dataAccess;
        }

        public async Task Log(LogMessage msg) //For built-in Discord.Net logging feature that logs to console and logfile
        {
            var cc = Console.ForegroundColor;
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    cc = ConsoleColor.DarkRed; break;
                case LogSeverity.Error:
                    cc = ConsoleColor.Red; break;
                case LogSeverity.Warning:
                    cc = ConsoleColor.Yellow; break;
                case LogSeverity.Info:
                    cc = ConsoleColor.White; break;
                case LogSeverity.Verbose:
                    cc = ConsoleColor.White; break;
                case LogSeverity.Debug:
                    cc = ConsoleColor.White; break;
            }
            await _logservice.Write(msg.ToString(), cc, TimeAppend.Short);
            await Task.CompletedTask;
        }

        public async Task MessageReceived(SocketMessage msg)
        {
            //Outputs to console and logs to logfile if the message starts with the s] prefix or is from StatusBot itself
            if (msg.Content.StartsWith("s]") || msg.Author.Id == 332603467577425929)
            {
                var ch = msg.Channel as IGuildChannel;
                var G = ch.Guild as IGuild;
                await _logservice.Write($"[{msg.Author}] {msg}", ConsoleColor.Green);
            }
            await Task.CompletedTask;
        }

        public async Task AutoSetGame()
        {
            await _client.SetGameAsync(DA.GetBotConfig(_client.CurrentUser).DefaultGame);
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
                    var U = Program.client.GetUser(listener.UserID);
                    var dmch = await U.GetOrCreateDMChannelAsync();
                    await dmch.SendMessageAsync($"{after} is offline at {DateTime.UtcNow} UTC");
                    await _logservice.Write($"Successfully PM'd {U} that {after} is offline", ConsoleColor.Cyan);
                }
            }
        }
    }
}