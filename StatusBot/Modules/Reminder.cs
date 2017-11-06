using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using StatusBot.Services.ReminderService;

namespace StatusBot.Modules.ReminderCommands
{
    public class ReminderModule : ModuleBase
    {
        private ReminderService RS = new ReminderService();
        public Discord.Color color = new Discord.Color(0, 138, 168);

        [Command("config")]
        [Summary("Check the current reminder configuration of a server")]
        [RequireContext(ContextType.Guild)]
        public async Task CheckConfig()
        {
            KeyValuePair<ulong?, ReminderConfig> guildCondig = RS.GetGuildConfig((SocketGuild)Context.Guild);
            string receivers = null;
            foreach (var id in guildCondig.Value.ReceiverID)
            {
                var U = await Context.Guild.GetUserAsync(id);
                receivers += $"{U.Id} | {U}\n";
            }

            string text = $"**Active**: {guildCondig.Value.Active}\n" +
                $"**Target**: {guildCondig.Value.TargetID} | {guildCondig.Value.TargetName}\n" +
                $"**Receivers**:\n{receivers}\n";

            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"{(SocketGuild)Context.Guild} reminder config")
                .WithDescription(text);

            await Context.Channel.SendMessageAsync("", embed: E);
        }

        [Command("activate")]
        [Summary("Activates the reminder")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Activate()
        {
            await RS.ModifyConfig((SocketGuild)Context.Guild, 1, null, null, null);
            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"Reminder activated");
            await Context.Channel.SendMessageAsync("", embed: E);
        }

        [Command("disactivate")]
        [Summary("Disativated the reminder")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Disactivate()
        {
            await RS.ModifyConfig((SocketGuild)Context.Guild, -1, null, null, null);
            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"Reminder disactivated");
            await Context.Channel.SendMessageAsync("", embed: E);
        }

        [Command("settarget")]
        [Summary("Sets the target bot")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task SetTarget([Remainder] SocketGuildUser T)
        {
            if (!T.IsBot) { await ReplyAsync("Non-bots cannot be set as a target"); return; }
            await RS.ModifyConfig((SocketGuild)Context.Guild, 0, T, null, null);
            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"Successfully set {T} as target");
            await Context.Channel.SendMessageAsync("", embed: E);
        }

        [Command("addreceiver")]
        [Summary("Adds one or more users as receivers")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task AddRec(params SocketGuildUser[] U)
        {
            if (!U.Any()) return;
            List<ulong> ids = U.Select(x => x.Id).ToList();
            await RS.ModifyConfig((SocketGuild)Context.Guild, 0, null, ids, "Add");
            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"Added {U.Count()} receivers");
            await Context.Channel.SendMessageAsync("", embed: E);
        }

        [Command("remreceiver")]
        [Summary("Remove one or more receivers")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task RemRec(params SocketGuildUser[] U)
        {
            if (!U.Any()) return;
            List<ulong> ids = U.Select(x => x.Id).ToList();
            await RS.ModifyConfig((SocketGuild)Context.Guild, 0, null, ids, "Rem");
            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"Removed {U.Count()} receivers");
            await Context.Channel.SendMessageAsync("", embed: E);
        }

        [Command("clrreceiver")]
        [Summary("Clears receivers list")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ClrRec()
        {
            List<ulong> bypass = new List<ulong>{};
            await RS.ModifyConfig((SocketGuild)Context.Guild, 0, null, bypass, "Clr");
            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"Removed all receivers");
            await Context.Channel.SendMessageAsync("", embed: E);
        }

        [Command("reset")]
        [Summary("Resets the entire configuration for the context server")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Reset()
        {
            List<ulong> bypass = new List<ulong> { 1 };
            await RS.ModifyConfig((SocketGuild)Context.Guild, -1, null, bypass, "Clr");
            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"Reminder successfully reset");
            await Context.Channel.SendMessageAsync("", embed: E);
        }

        [Command("refreshconfig")]
        [Summary("Fixes servers with missing configs. Owner only")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        public async Task RefreshConfig()
        {
            var client = Context.Client as DiscordSocketClient;
            var Guilds = client.Guilds;
            int count = 0;
            foreach (var guild in Guilds)
            {
                KeyValuePair<ulong?, ReminderConfig> guildconfig = RS.GetGuildConfig(guild);
                if (guildconfig.Key == null)
                {
                    await RS.CreateNewRegistry(guild);
                    count++;
                }
            }
            var E = new EmbedBuilder()
                .WithColor(new Discord.Color(200, 200, 200))
                .WithTitle($"Successfully refreshed {count} config(s)");
            await Context.Channel.SendMessageAsync("", embed: E);
        }
    }
}