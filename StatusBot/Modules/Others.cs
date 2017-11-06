using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace StatusBot.Modules
{
    public class Others : ModuleBase
    {
        public Discord.Color color = new Discord.Color(0, 138, 168);

        [Command("ping")]
        [Summary("Checks the latency of the bot")]
        [RequireContext(ContextType.Guild)]
        public async Task Ping()
        {
            var client = (DiscordSocketClient)Context.Client;
            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($":satellite_orbital: Beep!  |  {client.Latency}ms");
            await ReplyAsync("", embed: E);
        }

        [Command("about")]
        [Summary("See StatusBot's info")]
        [RequireContext(ContextType.Guild)]
        public async Task BotInfo()
        {
            var client = Context.Client as DiscordSocketClient;
            var cdate = client.CurrentUser.CreatedAt.DateTime;
            var E = new EmbedBuilder()
                .WithColor(color)
                .WithTitle("StatusBot's stats")
                .WithDescription("Hello there. I'm StatusBot. An ultra simple configurable bot to remind users of another bot going offline. " +
                "I'm built to assist on special cases and not intended for public use. But if you want to test around my capability in a server, please contact my creator")
                .AddInlineField("Creator", "StahlFerro#0055")
                .AddInlineField($"Creation date ({(DateTime.Now - cdate).Days}d old)", $"{cdate}")
                .AddInlineField("Library", "Discord.NET")
                .AddInlineField("Library Version", $"v{DiscordSocketConfig.Version}")
                .AddInlineField("Bot ID", 332603467577425929)
                .AddInlineField("Latency", client.Latency + "ms")
                //.AddInlineField("Links", $"[Bot invite](https://discordapp.com/oauth2/authorize?client_id=332603467577425929&scope=bot&permissions=117760)")
                ;
            await Context.Channel.SendMessageAsync("", embed: E);
        }

        [Command("how")]
        [Summary("Explains how to use my config and how I work")]
        [RequireContext(ContextType.Guild)]
        public async Task HowDoIWork()
        {
            await ReplyAsync(
                "Well, it's pretty simple. Each server has only one reminder config. The config consists of these 3 " +
                "variables: `Active`, `Target` and `Receiver`\n" +
                "**Active** : If true, then the reminder is set active. If disabled, then it's disabled duh\n" +
                "**Target** : The bot that will be ~~stalked~~ monitored by me\n" +
                "**Receiver** : One or more users that will be DM'd by me if the Target bot goes offline\n\n" +

                "So here's an example, when you set the following config on a server where " +
                "Active is set as `true`, Bot X is set as `Target`, You as `Receiver`, " +
                "I will DM you if the poor little has it's status set as offline aka dies. __The bot MUST be " +
                "in the same server where you set up this config or else I won't be able tell you that your precious " +
                "bot got rekt__\n\n" +

                "So that's all I can explain. Any further questions, please ask my creator.");
        }

        [Command("reboot")]
        [Summary("Restarts StatusBot. Creator only")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        public async Task Restart()
        {
            var E = new EmbedBuilder()
                .WithColor(new Color(200, 200, 200))
                .WithDescription("Restarting...");
            await Context.Channel.SendMessageAsync("", embed: E);
            Environment.Exit(1);
        }

        [Command("end")]
        [Summary("Shuts down StatusBot. Creator only")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        public async Task ShutDown()
        {
            var E = new EmbedBuilder()
                .WithColor(new Color(200, 200, 200))
                .WithDescription("Shutting down...");
            await Context.Channel.SendMessageAsync("", embed: E);
            Environment.Exit(0);
        }

        [Command("setgame")]
        [Summary("Sets the game of StatusBot. Creator only")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        public async Task SetGame([Remainder] string game = null)
        {
            var client = Context.Client as DiscordSocketClient;
            await client.SetGameAsync(game, streamType: StreamType.NotStreaming);
            if (game == null) await Context.Channel.SendMessageAsync("Successfully reset current game");
            else await Context.Channel.SendMessageAsync($"Successfully set game to {game}");
        }
    }
}
