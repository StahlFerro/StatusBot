using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net;

namespace StatusBot.Services
{
    public class TimerService
    {
        public ConcurrentDictionary<ulong, Timer> Timers;

        public TimerService(DiscordSocketClient client)
        {
            Timers = new ConcurrentDictionary<ulong, Timer>();
        }

        public bool DestroyTimer(ulong timer_id)
        {
            if (Timers.TryRemove(timer_id, out Timer timer))
            {
                timer.Dispose();
                return true;
            }
            else
                return false;
        }
    }
}
