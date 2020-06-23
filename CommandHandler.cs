using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using StatusBot.Services;
using StatusBot.TypeReaders;

namespace StatusBot
{
    public class CommandHandler
    {
        private CommandService C;
        private DiscordSocketClient client;
        private IServiceProvider ISP;
        private LogService LS;
        Stopwatch T = new Stopwatch();

        void SWatchStart()
            => T = Stopwatch.StartNew();

        void SWatchStop()
            => T.Stop();

        public CommandHandler(IServiceProvider provider)
        {
            ISP = provider;
            client = ISP.GetService<DiscordSocketClient>();
            client.MessageReceived += HandleCommand;
            C = ISP.GetService<CommandService>();
            LS = ISP.GetService<LogService>();
        }

        public async Task ConfigureAsync()
        {
            C.AddTypeReader(typeof(ChronoString), new ChronoStringTypeReader());
            await C.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: ISP);
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            SWatchStart();
            var ch = parameterMessage.Channel as IGuildChannel;
            var G = ch.Guild as IGuild;
            //Prevent commands triggered by itself or other bots
            if (!(parameterMessage is SocketUserMessage message) || message.Author.IsBot) return;
            int argPos = 0;
            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasStringPrefix("s]", ref argPos))) return;
            var context = new SocketCommandContext(client, message);
            var result = await C.ExecuteAsync(context, argPos, ISP);

            //Command success/fail notice message
            if (result.IsSuccess)
            {
                SWatchStop();
                await LS.WriteAsync($"Command finished in {T.Elapsed.TotalSeconds.ToString("F3")} seconds", ConsoleColor.DarkMagenta);
            }
            else
            {
                //Prevents unknown command exception to be posted as an error message in discord
                if (result.Error.Value != CommandError.UnknownCommand)
                    await message.Channel.SendMessageAsync(result.ToString());
                await LS.WriteErrorAsync($"{result}\n{result.ErrorReason}", ConsoleColor.DarkRed);
            }
        }
    }
}
