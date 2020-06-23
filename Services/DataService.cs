using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using Discord.WebSocket;
using StatusBot.Models;

namespace StatusBot.Services
{
    public class DataService
    {

        public Reminder GetReminderConfig(SocketGuild G, SocketGuildUser Bot)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                return SC.Reminders.FirstOrDefault(r => r.GuildId == G.Id && r.BotId == Bot.Id);
            }
        }

        public List<Reminder> GetGuildReminders(SocketGuild G)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                return SC.Reminders.AsQueryable().Where(rc => rc.GuildId == G.Id).ToList();
            }
        }

        public Listener GetListener(SocketGuild G, SocketGuildUser Bot, SocketGuildUser Listener)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                return GetListenerList(G, Bot).FirstOrDefault(l => l.UserID == Listener.Id);
            }
        }

        public List<Listener> GetListenerList(SocketGuild G, SocketGuildUser Bot)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                var RC = GetReminderConfig(G, Bot);
                var ReminderId = RC.ReminderId;
                return SC.Listeners.AsQueryable().Where(l => l.ReminderId == ReminderId).ToList();
            }
        }

        public List<Listener> GetListenerList(Reminder RC)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                var ReminderId = RC.ReminderId;
                return SC.Listeners.AsQueryable().Where(l => l.ReminderId == ReminderId).ToList();
            }
        }

        public async Task AddReminder(SocketGuild G, SocketGuildUser Bot, bool x, int duration)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                var RC = new Reminder
                {
                    GuildId = G.Id,
                    BotId = Bot.Id,
                    Active = x,
                    Duration = duration,
                };
                RC.Listeners = new List<Listener>() { };
                await SC.AddAsync(RC);
                await SC.SaveChangesAsync();
            }
        }

        public async Task ModifyReminderStatus(SocketGuild G, SocketGuildUser Bot, bool x)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {   
                //Not using GetReminder(G, Bot) in order for the entity to be tracked by the context to be updated
                var RC = SC.Reminders.FirstOrDefault(r => r.GuildId == G.Id && r.BotId == Bot.Id);
                RC.Active = x;
                await SC.SaveChangesAsync();
            }
        }

        public async Task ModifyReminderDuration(SocketGuild G, SocketGuildUser Bot, int duration)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                //Not using GetReminder(G, Bot) in order for the entity to be tracked by the context to be updated
                var RC = SC.Reminders.FirstOrDefault(r => r.GuildId == G.Id && r.BotId == Bot.Id);
                RC.Duration = duration;
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
                var L = new Listener
                {
                    UserID = Listener.Id,
                    ReminderId = GetReminderConfig(G, Bot).ReminderId,
                };
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

        public BotConfig GetBotConfig(SocketSelfUser Bot)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                return SC.BotConfigs.FirstOrDefault(c => c.BotId == Bot.Id);
            }
        }

        public async Task<List<List<string>>> ExecSql(string query)
        {
            using (StatusBotContext SC = new StatusBotContext())
            {
                var db = SC.Database;
                DbCommand command = db.GetDbConnection().CreateCommand();
                command.CommandText = query;
                await db.OpenConnectionAsync();
                DbDataReader reader = await command.ExecuteReaderAsync();

                var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                var records = new List<List<string>>();
                records.Add(columnNames);
                while (await reader.ReadAsync())
                {
                    var row = new List<string>();
                    foreach (var colname in columnNames)
                    {
                        string data = (reader[colname].ToString().EscapeCSV());
                        row.Add(data);
                    }
                    records.Add(row);
                }
                reader.Close();
                return records;
            }
        }
    }
}