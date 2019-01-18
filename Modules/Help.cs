using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace StatusBot.Modules
{
    public class Help : ModuleBase
    {
        private CommandService C;
        public Help(CommandService service)
        {
            C = service;
        }

        [Command("help")]
        [Alias("h")]
        [RequireContext(ContextType.Guild)]
        public async Task HelpModule([Remainder] string command = null)
        {
            if (String.IsNullOrWhiteSpace(command))
            {
                var realtotal = C.Commands.Count();
                var E = new EmbedBuilder()
                    .WithColor(new Color(0, 138, 168))
                    .WithDescription("These are all of my available commands. Type s]help `command` for more info");
                int number = 1;
                int totalc = 0;
                var modules = C.Modules.OrderBy(x => x.Name);
                foreach (var module in modules)
                {
                    if (module.Name == "Help" || module.Name == "BotOwner") { continue; }
                    string description = null;
                    var commands = module.Commands.OrderBy(x => x.Name);
                    foreach (var c in commands)
                    {
                        //var result = await c.CheckPreconditionsAsync(Context);
                        //if (result.IsSuccess)
                        description += $"`{c.Name}`   ";
                    }
                    if (!String.IsNullOrWhiteSpace(description))
                    {
                        E.AddField($"{number}.  {module.Name}", description);
                    }
                    totalc += commands.Count();
                    number++;
                }
                E.WithTitle($"StatusBot Commands ({totalc} available)");
                await ReplyAsync("", false, E.Build());
            }

            else
            {
                var result = C.Search(Context, command);
                if (!result.IsSuccess)
                {
                    await ReplyAsync($"Sorry, I couldn't find any commands that matches **{command}**."); return;
                }

                var E = new EmbedBuilder().WithColor(new Discord.Color(0, 138, 168));
                foreach (var match in result.Commands)
                {
                    var c = match.Command;
                    var ca = c.Aliases.Select(x => "s]" + x.ToString()).ToArray();
                    E.WithTitle(String.Join(" / ", ca));
                    E.WithDescription(c.Summary);
                    //E.AddField("Parameters: ", $"{String.Join(", ", c.Parameters.Select(p => p.Name))}");
                }
                await ReplyAsync("", false, E.Build());
            }
        }

    }
}
