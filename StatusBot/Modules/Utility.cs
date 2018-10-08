using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net;
using StatusBot.Services;


namespace StatusBot.Modules.Utility
{
    public class Utility : ModuleBase<SocketCommandContext>
    {
        public Discord.Color color;
        private IServiceProvider ISP;
        private DataService DA;

        public Utility(IServiceProvider provider)
        {
            color = new Discord.Color(0, 138, 168);
            ISP = provider;
            DA = ISP.GetService<DataService>();
        }

        [Command("ping")]
        [Summary("Checks the latency of the bot")]
        [RequireContext(ContextType.Guild)]
        public async Task Ping()
        {
            var client = Context.Client;
            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($":satellite_orbital: Beep!  |  {client.Latency}ms");
            await ReplyAsync("", embed: E.Build());
        }

        [Command("about")]
        [Summary("See StatusBot's info")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(GuildPermission.SendMessages)]
        public async Task BotInfo()
        {
            var client = Context.Client;
            var cdate = client.CurrentUser.CreatedAt.DateTime;
            var botconfig = DA.GetBotConfig(client.CurrentUser);
            var hq = client.GetGuild(botconfig.HeadquartersGuildId);
            //var invs = await hq.GetInvitesAsync();
            //var non_expire_inv = invs.FirstOrDefault(i => i.MaxAge == null);
            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"StatusBot v{botconfig.VersionNumber}")
                .WithDescription("Hello there. I'm StatusBot. An ultra simple configurable bot to remind users of another bot going offline. " +
                "I'm built to assist on special cases and not intended for public use. But if you want to test around my capability in a server, please contact my creator")
                .AddField("Creator", "StahlFerro#0055", inline: true)
                .AddField($"Creation date ({(DateTime.Now - cdate).Days}d old)", $"{cdate}", inline: true)
                .AddField("Library", $"<:dotnet:315951014156959744> Discord.NET v{DiscordSocketConfig.Version}", inline: true)
                .AddField("Bot ID", Context.Client.CurrentUser.Id, inline: true)
                .AddField("Latency", client.Latency + "ms", inline: true)
                .AddField("Links", $"[Github](https://github.com/StahlFerro/StatusBot)", inline: true);
            await Context.Channel.SendMessageAsync("", embed: E.Build());
        }

        [Command("changelog")]
        [Summary("Displays the latest changelog of the bot")]
        [RequireBotPermission(GuildPermission.SendMessages)]
        public async Task ChangeLog()
        {
            var client = Context.Client;
            var hq = client.GetGuild(306467828729380874);
            var changelogch = hq.TextChannels.FirstOrDefault(ch => ch.Name == "changelog");
            var changelogmessages = await changelogch.GetMessagesAsync(100).FlattenAsync();
            var msg = changelogmessages.FirstOrDefault(m => m.Content.StartsWith("**StatusBot")).Content;
            await ReplyAsync(msg);
        }

        [Command("how")]
        [Summary("Explains how to use my reminders and how I work")]
        [RequireContext(ContextType.Guild)]
        public async Task HowDoIWork()
        {
            await ReplyAsync(
                "Well, it's pretty simple. Each server can have one or more reminders. A reminder consists of these 3 " +
                "variables: `Active`, `Bot` and `Listeners`\n" +
                "**Active** : If true, then the reminder is set active. If disabled, then it's disabled duh\n" +
                "**Bot** : The bot that will be ~~stalked~~ monitored by me\n" +
                "**Listeners** : One or more users that will be DM'd by me if the Bot goes offline\n\n" +

                "So here's an example, when you set the following config on a server where " +
                "Active is set as `true`, SomeBot is set as `Bot`, You as `Listener`, " +
                "I will DM you if the poor little thing has it's status set as offline aka dies. __The bot MUST be " +
                "in the same server where you set up this config or else I won't be able tell you that your precious " +
                "bot got rekt__\n\n" +

                "So that's all I can explain. Any further questions, please ask my creator.");
        }
    }
}
