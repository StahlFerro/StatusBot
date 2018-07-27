using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using StatusBot.Services;

namespace StatusBot
{
    public class CommandHandler
    {
        private CommandService C;
        private DiscordSocketClient client;
        private IServiceProvider ISP;
        private LogService _logservice;
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
            _logservice = ISP.GetService<LogService>();
        }

        public async Task ConfigureAsync()
        {
            await C.AddModulesAsync(Assembly.GetEntryAssembly());
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
            var context = new CommandContext(client, message);
            var result = await C.ExecuteAsync(context, argPos, ISP);

            //Command success/fail notice message
            if (result.IsSuccess)
            {
                SWatchStop();
                await _logservice.Write($"Command finished in {T.Elapsed.TotalSeconds.ToString("F3")} seconds", ConsoleColor.DarkMagenta);
            }
            else
            {
                //Prevents unknown command exception to be posted as the error message in discord
                if (result.Error.Value != CommandError.UnknownCommand)
                    await message.Channel.SendMessageAsync(result.ToString());
                await _logservice.Write($"{result}", ConsoleColor.DarkRed);
            }
        }
    }
}
