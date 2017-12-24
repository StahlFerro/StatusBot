using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using StatusBot.Models;
using StatusBot.Services;

namespace StatusBot.Modules
{
    public class Reminder : ModuleBase
    {
        public Discord.Color color = new Discord.Color(0, 138, 168);
        DataAccess DA = new DataAccess();

        [Command("rmd")]
        [Summary("Displays guild reminders")]
        [RequireContext(ContextType.Guild)]
        public async Task Reminders([Remainder] SocketGuildUser Bot = null)
        {
            try
            {
                var client = Context.Client as DiscordSocketClient;
                var G = Context.Guild as SocketGuild;
                string desc = "";
                var E = new EmbedBuilder().WithColor(color);
                if (Bot == null)
                {
                    List<REMINDERCONFIG> guildreminders = DA.GetGuildReminders(G);
                    foreach (var rc in guildreminders)
                    {
                        List<LISTENER> listeners = DA.GetListeners(rc);
                        string rcstatus;
                        if (rc.Active) rcstatus = "Active";
                        else rcstatus = "Inactive";
                        desc += $"{G.GetUser(Convert.ToUInt64(rc.BotID))} ({rc.BotID}) | {rcstatus} | {listeners.Count} listener\n";
                    }
                    if (String.IsNullOrWhiteSpace(desc)) desc = "No reminders yet. Cri";
                    E.WithTitle($"{G.Name} reminders").WithDescription(desc);
                }
                else
                {
                    REMINDERCONFIG reminder = DA.GetReminderConfig(G, Bot);
                    if (reminder == null) { await ReplyAsync("Reminder not found"); return; }
                    foreach (var listener in DA.GetListeners(G, Bot))
                    {
                        desc += $"{G.GetUser(Convert.ToUInt64(listener.UserID))} ({listener.UserID})\n";
                    }
                    string rcstatus;
                    if (reminder.Active) rcstatus = "Active";
                    else rcstatus = "Inactive";
                    E.WithTitle($"Listeners of {Bot} ({Bot.Id}) | {rcstatus}").WithDescription(desc);
                }
                await ReplyAsync("", embed: E);
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message + "\n" + e.StackTrace);
            }
        }

        [Command("addrmd")]
        [Summary("Adds a new reminder to the list, or if already exists, modifies it's active status")]
        [RequireContext(ContextType.Guild)]
        public async Task AddRmd(SocketGuildUser Bot, [Remainder] bool x = false)
        {
            if (!Bot.IsBot) { await ReplyAsync("Reminder must be a bot!"); return; }
            string status = "";
            if (x) status = "Activated";
            else status = "Deactivated";
            var G = Context.Guild as SocketGuild;
            if (DA.GetReminderConfig(G, Bot) == null) //If the reminder doesn't exist
            {
                await DA.AddReminder(G, Bot, x);
                await ReplyAsync($"Added new reminder: {Bot} ({Bot.Id}) and {status}");
            }
            else
            {
                await DA.ModifyReminderStatus(G, Bot, x);
                await ReplyAsync($"{status} reminder: {Bot} ({Bot.Id})");
            }
        }

        [Command("modrmd")]
        [Summary("Modifies a reminder's active status")]
        [RequireContext(ContextType.Guild)]
        public async Task ModRmd(SocketGuildUser Bot, [Remainder] bool x = false)
        {
            string status = "";
            if (x) status = "Activated";
            else status = "Deactivated";
            var G = Context.Guild as SocketGuild;
            if (DA.GetReminderConfig(G, Bot) == null) //If the reminder doesn't exist
            {
                await ReplyAsync("⛔ The reminder to be modified is not on the list!");
            }
            else
            {
                await DA.ModifyReminderStatus(G, Bot, x);
                await ReplyAsync($"{status} reminder: {Bot} ({Bot.Id})");
            }
        }

        [Command("delrmd")]
        [Summary("Removes a reminder from the list")]
        [RequireContext(ContextType.Guild)]
        public async Task DelRmd([Remainder] SocketGuildUser Bot)
        {
            var G = Context.Guild as SocketGuild;
            if (DA.GetReminderConfig(G, Bot) == null)
            {
                await ReplyAsync("⛔ The reminder to be deleted is not on the list!");
            }
            else
            {
                await DA.DelReminder(G, Bot);
                await ReplyAsync($"Deleted new bot: {Bot} ({Bot.Id})");
            }
        }

        [Command("addlist")]
        [Summary("Adds a new listener to a reminder")]
        [RequireContext(ContextType.Guild)]
        public async Task AddList(SocketGuildUser Bot, [Remainder] SocketGuildUser Listener)
        {
            var G = Context.Guild as SocketGuild;
            if (DA.GetListener(G, Bot, Listener) == null)
            {
                await DA.AddListener(G, Bot, Listener);
                await ReplyAsync($"Added new listener to {Bot}: {Listener} {Listener.Id}");
            }
            else
            {
                await ReplyAsync($"⛔ same listener already exists for this reminder");
            }
        }

        [Command("dellist")]
        [Summary("Deletes a listener from a reminder")]
        [RequireContext(ContextType.Guild)]
        public async Task DelList(SocketGuildUser Bot, [Remainder] SocketGuildUser Listener)
        {
            var G = Context.Guild as SocketGuild;
            if (DA.GetListener(G, Bot, Listener) == null)
            {
                await ReplyAsync($"⛔ the listener does not exist for this reminder");
            }
            else
            {
                await DA.DelListener(G, Bot, Listener);
                await ReplyAsync($"Deleted listener from {Bot}: {Listener} {Listener.Id}");
            }
        }
    }
}
