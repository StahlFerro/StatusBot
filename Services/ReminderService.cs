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
            var G = before.Guild;
            var R = DS.GetReminderConfig(G, before);
            if (!R.Active)
                return;
            var listenerlist = DS.GetListenerList(G, before);
            if (!listenerlist.Any()) return;
            TimeSpan duration = TimeSpan.FromSeconds(R.Duration);
            Timer ping_timer = new Timer(async _ => {
                await PingListeners(after, listenerlist);
                TS.DestroyTimer(R.ReminderId);
            }, null, duration, Timeout.InfiniteTimeSpan);

            TS.Timers.TryAdd(R.ReminderId, ping_timer);
            await Task.CompletedTask;
        }

        public async Task CancelReminder(SocketGuildUser bot)
        {
            var G = bot.Guild;
            var R = DS.GetReminderConfig(G, bot);
            if (R != null)
            {
                TS.DestroyTimer(R.ReminderId);
                await LS.Write($"Cancelled reminder for:\n" +
                    $"ReminderId: {R.ReminderId} | BotId: {R.BotId}");
            }
            await Task.CompletedTask;
        }

        private async Task PingListeners(SocketGuildUser after, ICollection<Listener> listeners)
        {
            List<Task> ping_tasks = new List<Task>();
            List<Task> log_tasks = new List<Task>();

            foreach (var listener in listeners)
            {
                var U = _client.GetUser(listener.UserID);
                if (U == null)
                {
                    await LS.Write($"Listener with UserID: {listener.UserID} in server {after.Guild} ({after.Guild.Id})");
                    return;
                }
                var dmch = await U.GetOrCreateDMChannelAsync();
                ping_tasks.Add(dmch.SendMessageAsync($"{after} is offline at {DateTime.UtcNow} UTC"));
                log_tasks.Add(LS.Write($"Successfully PM'd {U} that {after} is offline", ConsoleColor.Cyan));
            }

            await Task.WhenAll(ping_tasks);
            await Task.WhenAll(log_tasks);
        }
    }
}
