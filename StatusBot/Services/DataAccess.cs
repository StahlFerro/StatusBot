using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using Discord.WebSocket;
using StatusBot.Models;

namespace StatusBot.Services
{
    public class DataAccess
    {
        public REMINDERCONFIG GetReminderConfig(SocketGuild G, SocketGuildUser Bot)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                return SC.REMINDERCONFIGs.FirstOrDefault(
                    r => r.GuildID == Convert.ToInt64(G.Id) && r.BotID == Convert.ToInt64(Bot.Id));
            }
        }

        public List<REMINDERCONFIG> GetGuildReminders(SocketGuild G)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                return SC.REMINDERCONFIGs.ToList();
            }
        }

        public LISTENER GetListener(SocketGuild G, SocketGuildUser Bot, SocketGuildUser Listener)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                return GetListeners(G, Bot).FirstOrDefault(l => l.UserID == Convert.ToInt64(Listener.Id));
            }
        }

        public List<LISTENER> GetListeners(SocketGuild G, SocketGuildUser Bot)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                var RC = GetReminderConfig(G, Bot);
                var ReminderId = RC.ReminderID;
                return SC.LISTENERs.Where(l => l.ReminderIDFK == ReminderId).ToList();
            }
        }

        public List<LISTENER> GetListeners(REMINDERCONFIG RC)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                var ReminderId = RC.ReminderID;
                return SC.LISTENERs.Where(l => l.ReminderIDFK == ReminderId).ToList();
            }
        }

        public async Task AddReminder(SocketGuild G, SocketGuildUser Bot, bool x)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                var RC = new REMINDERCONFIG();
                RC.GuildID = Convert.ToInt64(G.Id);
                RC.BotID = Convert.ToInt64(Bot.Id);
                RC.Active = x;
                RC.ListenerList = new List<LISTENER>() { };
                await SC.AddAsync(RC);
                await SC.SaveChangesAsync();
            }
        }

        public async Task ModifyReminderStatus(SocketGuild G, SocketGuildUser Bot, bool x)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {   
                //Not using GetReminder(G, Bot) in order for the entity to be tracked by the context to be updated
                var RC = SC.REMINDERCONFIGs.FirstOrDefault( 
                    r => r.GuildID == Convert.ToInt64(G.Id) && r.BotID == Convert.ToInt64(Bot.Id));
                RC.Active = x;
                await SC.SaveChangesAsync();
            }
        }

        public async Task DelReminder(SocketGuild G, SocketGuildUser Bot)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                var RC = GetReminderConfig(G, Bot);
                SC.Remove(RC);
                await SC.SaveChangesAsync();
            }
        }

        public async Task AddListener(SocketGuild G, SocketGuildUser Bot, SocketGuildUser Listener)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                var L = new LISTENER();
                L.UserID = Convert.ToInt64(Listener.Id);
                L.ReminderIDFK = GetReminderConfig(G, Bot).ReminderID;
                await SC.AddAsync(L); //Adds the Listener to the LISTENERs table
                await SC.SaveChangesAsync();
            }
        }

        public async Task DelListener(SocketGuild G, SocketGuildUser Bot, SocketGuildUser Listener)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                var L = GetListener(G, Bot, Listener);
                SC.Remove(L);
                await SC.SaveChangesAsync();
            }
        }

        public async Task AutoPM(SocketGuildUser before, SocketGuildUser after)
        {
            var G = before.Guild;
            var reminder = GetReminderConfig(G, before);
            if (before.IsBot && reminder.Active && after.Status == UserStatus.Offline)
            {
                foreach (var listener in GetListeners(G, before))
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