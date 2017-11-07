using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace StatusBot
{
    public class CommandHandler
    {
        private CommandService C;
        private DiscordSocketClient client;
        private IServiceProvider ISP;
        Stopwatch T = new Stopwatch();

        void SWatchStart()
        {
            T = Stopwatch.StartNew();
        }
        void SWatchStop()
        {
            T.Stop();
            Console.WriteLine($"Command finished in {T.Elapsed.TotalSeconds.ToString("F3")} seconds");
            File.AppendAllText("logfile.txt", $"Command finished in {T.Elapsed.TotalSeconds.ToString("F3")} seconds\n");
        }

        public CommandHandler(IServiceProvider provider)
        {
            ISP = provider;
            client = ISP.GetService<DiscordSocketClient>();
            client.MessageReceived += HandleCommand;
            C = ISP.GetService<CommandService>();
        }

        public async Task ConfigureAsync()
        {
            await C.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            SWatchStart();
            var message = parameterMessage as SocketUserMessage;
            var ch = parameterMessage.Channel as IGuildChannel;
            var G = ch.Guild as IGuild;
            //Prevent commands triggered by itself or other bots
            if (message == null || message.Author.IsBot) return;
            int argPos = 0;
            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasStringPrefix("s]", ref argPos))) return;
            var context = new CommandContext(client, message);
            var result = await C.ExecuteAsync(context, argPos, ISP);

            //Command success/fail notice message
            if (result.IsSuccess)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                SWatchStop();
                Console.ResetColor();
            }
            else
            {
                //Prevents unknown command exception to be posted as the error message in discord
                if (result.Error.Value != CommandError.UnknownCommand)
                    await message.Channel.SendMessageAsync(result.ErrorReason);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"{result.ToString()}");
                Console.ResetColor();
                File.AppendAllText("logfile.txt", $"{result.ToString()}\n");
            }
        }
    }
}
