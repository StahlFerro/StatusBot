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
using StatusBot.TypeReaders;

namespace StatusBot.Modules
{
    public enum Switch { Off, On, None };

    public class ReminderCommand : ModuleBase<SocketCommandContext>
    {
        public Discord.Color color;

        private DataService DS;

        public ReminderCommand(IServiceProvider ISP)
        {
            DS = ISP.GetService<DataService>();
            color = new Color(0, 138, 168);
        }

        [Command("rmd")]
        [Summary("Displays guild bot reminders")]
        [RequireContext(ContextType.Guild)]
        public async Task Reminders([Remainder] SocketGuildUser Bot = null)
        {
            var client = Context.Client;
            var G = Context.Guild as SocketGuild;
            string desc = "";
            var E = new EmbedBuilder().WithColor(color);
            if (Bot == null)
            {
                List<Reminder> guildreminders = DS.GetGuildReminders(G);
                foreach (var rc in guildreminders)
                {
                    List<Listener> listenerlist = DS.GetListenerList(rc);
                    string rcstatus;
                    if (rc.Active) rcstatus = "Active";
                    else rcstatus = "Inactive";
                    desc += $"{G.GetUser(rc.BotId)} ({rc.BotId}) | {rcstatus} ({rc.Duration}s) | {listenerlist.Count} listener(s)\n";
                }
                if (string.IsNullOrWhiteSpace(desc)) desc = "No bot reminders yet. Cri";
                E.WithTitle($"{G.Name} reminders").WithDescription(desc);
            }
            else
            {
                Reminder reminder = DS.GetReminderConfig(G, Bot);
                if (reminder == null) { await ReplyAsync("⛔ Reminder not found"); return; }
                foreach (var listener in DS.GetListenerList(G, Bot))
                {
                    desc += $"{G.GetUser(listener.UserID)} ({listener.UserID})\n";
                }
                if (string.IsNullOrWhiteSpace(desc)) desc = "No listeners for this reminder yet. Cri";
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
            "`s]modrmd MyBot 2s` adds as well as activating the reminder")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AddRmd(SocketGuildUser bot, [Remainder] ChronoString CS = null)
        {
            if (!bot.IsBot) { await ReplyAsync("⛔ Reminder must be a bot!"); return; }
            var G = Context.Guild;
            double duration = CS == null ? 0d : CS.Time.TotalSeconds;
            if (duration > 300d) { await ReplyAsync("⛔ Reminder duration must not be greater than 5 minutes!"); return; }
            if (DS.GetReminderConfig(G, bot) == null) //If the reminder doesn't exist
            {
                await DS.AddReminder(G, bot, false, (int)duration);
                await ReplyAsync($"✅ Added new bot reminder: {bot} ({bot.Id}) with duration: {(int)duration} seconds");
            }
            else
            {
                await ReplyAsync("⛔ A reminder with the same target bot already exists on the list!");
            }
        }

        [Command("switchrmd")]
        [Summary("Activates or deactivates a bot reminder.\nExamples:\n" +
            "`s]switchrmd MyBot on` deactivates the reminder\n" +
            "`s]switchrmd MyBot off` activates the reminder")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task SwitchRmd(SocketGuildUser bot, [Remainder] Switch sw = Switch.None)
        {
            string status = "";
            bool active = false;
            if (sw == Switch.None) { await ReplyAsync("⛔ Please specify On or Off!"); return; }
            else if (sw == Switch.On)
            {
                status = "Activated";
                active = true;
            }
            else if (sw == Switch.Off)
            {
                status = "Deactivated";
            }
            var G = Context.Guild;
            if (DS.GetReminderConfig(G, bot) == null) //If the reminder doesn't exist
            {
                await ReplyAsync("⛔ The bot reminder to be modified is not on the list!");
            }
            else
            {
                await DS.ModifyReminderStatus(G, bot, active);
                await ReplyAsync($"✅ {status} Reminder: {bot} ({bot.Id})");
            }
        }

        [Command("durationrmd")]
        [Summary("Modifies the duration of a bot reminder\nExamples:\n" +
            "`]!durationrmd MyBot` Sets the duration to 0 seconds (default)\n" +
            "`]!durationrmd MyBot 12s` Sets the duration to 12 seconds")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task DurationRmd(SocketGuildUser bot, [Remainder] ChronoString CS = null)
        {
            double duration = CS == null ? 0d : CS.Time.TotalSeconds;
            if (duration > 300d) { await ReplyAsync("⛔ Reminder duration must not be greater than 5 minutes!"); return; }
            else
            {
                await DS.ModifyReminderDuration(Context.Guild, bot, (int)duration);
                await ReplyAsync($"✅ New duration: {(int)duration} seconds. Reminder: {bot} ({bot.Id})");
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
            var G = Context.Guild;
            if (DS.GetReminderConfig(G, bot) == null)
            {
                await ReplyAsync("⛔ The bot reminder to be deleted is not on the list!");
            }
            else
            {
                await DS.DelReminder(G, bot);
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
            listener = listener ?? Context.User as SocketGuildUser;
            if (listener.IsBot) { await ReplyAsync("⛔ The listener must be a user, not a bot!"); return; }

            var G = Context.Guild;
            if (DS.GetListener(G, bot, listener) == null)
            {
                await DS.AddListener(G, bot, listener);
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
            var G = Context.Guild;
            if (DS.GetListener(G, bot, listener) == null)
            {
                await ReplyAsync($"⛔ The listener does not exist for this bot reminder");
            }
            else
            {
                await DS.DelListener(G, bot, listener);
                await ReplyAsync($"✅ Deleted listener from {bot}: {listener} ({listener.Id})");
            }
        }
    }
}