using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using StatusBot.Models;
using StatusBot.Services;

namespace StatusBot.Modules
{
    public class Reminder : ModuleBase
    {
        public Discord.Color color;

        private DataAccess DA;

        public Reminder(IServiceProvider ISP)
        {
            DA = ISP.GetService<DataAccess>();
            color = new Discord.Color(0, 138, 168);
        }

        [Command("rmd")]
        [Summary("Displays guild bot reminders")]
        [RequireContext(ContextType.Guild)]
        public async Task Reminders([Remainder] SocketGuildUser Bot = null)
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
                    List<LISTENER> listenerlist = DA.GetListenerList(rc);
                    string rcstatus;
                    if (rc.Active) rcstatus = "Active";
                    else rcstatus = "Inactive";
                    desc += $"{G.GetUser(rc.BotID)} ({rc.BotID}) | {rcstatus} | {listenerlist.Count} listener(s)\n";
                }
                if (String.IsNullOrWhiteSpace(desc)) desc = "No bot reminders yet. Cri";
                E.WithTitle($"{G.Name} reminders").WithDescription(desc);
            }
            else
            {
                REMINDERCONFIG reminder = DA.GetReminderConfig(G, Bot);
                if (reminder == null) { await ReplyAsync("⛔ Reminder not found"); return; }
                foreach (var listener in DA.GetListenerList(G, Bot))
                {
                    desc += $"{G.GetUser(listener.UserID)} ({listener.UserID})\n";
                }
                if (String.IsNullOrWhiteSpace(desc)) desc = "No listeners for this reminder yet. Cri";
                string rcstatus;
                if (reminder.Active) rcstatus = "Active";
                else rcstatus = "Inactive";
                E.WithTitle($"Listeners of {Bot} ({Bot.Id}) | {rcstatus}").WithDescription(desc);
            }
            await ReplyAsync("", embed: E.Build());
        }

        [Command("addrmd")]
        [Summary("Adds a new bot reminder to the list\nExamples:\n" +
            "`s]addrmd MyBot` adds a bot to be tracked for it's offline status, reminder deactivated by default\n" +
            "`s]modrmd MyBot true` adds as well as activating the reminder")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AddRmd(SocketGuildUser bot, [Remainder] bool x = false)
        {
            if (!bot.IsBot) { await ReplyAsync("⛔ Reminder must be a bot!"); return; }
            string status = "";
            if (x) status = "Activated";
            else status = "Deactivated";
            var G = Context.Guild as SocketGuild;
            if (DA.GetReminderConfig(G, bot) == null) //If the reminder doesn't exist
            {
                await DA.AddReminder(G, bot, x);
                await ReplyAsync($"✅ Added new bot reminder: {bot} ({bot.Id}) and {status}");
            }
            else
            {
                await ReplyAsync("⛔ A reminder with the same target bot already exists on the list!");
            }
        }

        [Command("modrmd")]
        [Summary("Activates or deactivates a bot reminder.\nExamples:\n" +
            "`s]modrmd MyBot false` deactivates the reminder\n" +
            "`s]modrmd MyBot true` activates the reminder")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task ModRmd(SocketGuildUser bot, [Remainder] bool x = false)
        {
            string status = "";
            if (x) status = "Activated";
            else status = "Deactivated";
            var G = Context.Guild as SocketGuild;
            if (DA.GetReminderConfig(G, bot) == null) //If the reminder doesn't exist
            {
                await ReplyAsync("⛔ The bot reminder to be modified is not on the list!");
            }
            else
            {
                await DA.ModifyReminderStatus(G, bot, x);
                await ReplyAsync($"✅ {status} reminder: {bot} ({bot.Id})");
            }
        }

        [Command("delrmd")]
        [Summary("Removes a bot reminder from the list\nExample:" +
            "`s]delrmd MyBot`")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task DelRmd([Remainder] SocketGuildUser bot)
        {
            var G = Context.Guild as SocketGuild;
            if (DA.GetReminderConfig(G, bot) == null)
            {
                await ReplyAsync("⛔ The bot reminder to be deleted is not on the list!");
            }
            else
            {
                await DA.DelReminder(G, bot);
                await ReplyAsync($"✅ Deleted new bot reminder: {bot} ({bot.Id})");
            }
        }

        [Command("addlist")]
        [Summary("Adds a new listener to a reminder\nExamples:\n" +
            "`s]addlist MyBot` assigns yourself to be reminded of MyBot going offline\n" +
            "`s]addlist MyBot Han` assigns another user to be reminded of MyBot going offline")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AddList(SocketGuildUser bot, [Remainder] SocketGuildUser listener = null)
        {
            if (listener.IsBot) { await ReplyAsync("⛔ The listener must be a user, not a bot!"); return; }
            listener = listener ?? Context.User as SocketGuildUser;
            var G = Context.Guild as SocketGuild;
            if (DA.GetListener(G, bot, listener) == null)
            {
                await DA.AddListener(G, bot, listener);
                await ReplyAsync($"✅ Added new listener to {bot}: {listener} ({listener.Id})");
            }
            else
            {
                await ReplyAsync($"⛔ The listener already exists for this bot reminder");
            }
        }

        [Command("dellist")]
        [Summary("Deletes a listener from a reminder\nExamples:\n" +
            "`s]dellist MyBot` removes yourself from the MyBot reminder\n" +
            "`s]dellist MyBot Solo` removes another user from the MyBot reminder")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task DelList(SocketGuildUser bot, [Remainder] SocketGuildUser listener = null)
        {
            listener = listener ?? Context.User as SocketGuildUser;
            var G = Context.Guild as SocketGuild;
            if (DA.GetListener(G, bot, listener) == null)
            {
                await ReplyAsync($"⛔ The listener does not exist for this bot reminder");
            }
            else
            {
                await DA.DelListener(G, bot, listener);
                await ReplyAsync($"✅ Deleted listener from {bot}: {listener} ({listener.Id})");
            }
        }
    }
}