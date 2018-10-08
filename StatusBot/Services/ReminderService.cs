using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using StatusBot.Models;

namespace StatusBot.Services
{
    public class ReminderService
    {
        private readonly DiscordSocketClient _client;
        private readonly TimerService TS;
        private readonly DataService DS;
        private readonly LogService LS;

        public ReminderService(DiscordSocketClient client, TimerService timerService, DataService dataService, LogService logService)
        {
            _client = client;
            TS = timerService;
            DS = dataService;
            LS = logService;
        }

        public async Task RemindUsers(SocketGuildUser before, SocketGuildUser after)
        {
            Console.WriteLine("triggered");
            var G = before.Guild;
            Console.WriteLine(G.Name);
            var R = DS.GetReminderConfig(G, before);
            Console.WriteLine("ok?");
            Console.WriteLine((R == null).ToString());
            Console.WriteLine($"{R.ReminderId} {R.GuildId} {R.BotId} {R.Duration}");
            if (!R.Active)
                return;
            var listenerlist = DS.GetListenerList(G, before);
            Console.WriteLine(string.Join(", ", listenerlist.Select(l => l.UserID)));
            if (!listenerlist.Any()) return;
            TimeSpan duration = TimeSpan.FromSeconds(R.Duration);
            Timer ping_timer = new Timer(async _ => {
                await PingListeners(after, listenerlist);
            }, null, duration, Timeout.InfiniteTimeSpan);
            await Task.CompletedTask;
        }

        private async Task PingListeners(SocketGuildUser after, ICollection<Listener> listeners)
        {
            List<Task> ping_tasks = new List<Task>();
            List<Task> log_tasks = new List<Task>();

            foreach (var listener in listeners)
            {
                var U = Program.client.GetUser(listener.UserID);
                var dmch = await U.GetOrCreateDMChannelAsync();
                ping_tasks.Add(dmch.SendMessageAsync($"{after} is offline at {DateTime.UtcNow} UTC"));
                log_tasks.Add(LS.Write($"Successfully PM'd {U} that {after} is offline", ConsoleColor.Cyan));
            }

            await Task.WhenAll(ping_tasks);
            await Task.WhenAll(log_tasks);
        }
    }
}
