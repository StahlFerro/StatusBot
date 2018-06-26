using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net;
using System.Diagnostics;


namespace StatusBot.Modules
{
    public class BotOwner : ModuleBase
    {
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

        [Command("setpresence")]
        [Summary("Sets the game of StatusBot. Creator only")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        public async Task SetPresence(string option, [Remainder] string game = null)
        {
            var client = Context.Client as DiscordSocketClient;
            var presence = StreamType.NotStreaming;
            if (option == "-game") presence = StreamType.NotStreaming;
            else if (option == "-stream") presence = StreamType.Twitch;
            else {await ReplyAsync("Unknown stream type"); return;}
            await client.SetGameAsync(game, streamType: presence);
            if (game == null) await Context.Channel.SendMessageAsync("Successfully reset current presence");
            else await Context.Channel.SendMessageAsync($"Successfully set presence to {game}");
        }
    }
}
