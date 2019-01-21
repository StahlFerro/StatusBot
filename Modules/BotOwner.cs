using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net;
using System.Diagnostics;
using StatusBot.Services;


namespace StatusBot.Modules
{
    public class BotOwner : ModuleBase<SocketCommandContext>
    {
        private DataService DA;
        private TimerService TS;
        readonly Random R;
        private CommandService CS;
        public BotOwner(IServiceProvider ISP)
        {
            DA = ISP.GetService<DataService>();
            TS = ISP.GetService<TimerService>();
            R = new Random();
            CS = ISP.GetService<CommandService>();
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
            await Context.Channel.SendMessageAsync("", embed: E.Build());
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
            await Context.Channel.SendMessageAsync("", embed: E.Build());
            Environment.Exit(0);
        }

        [Command("setgame")]
        [Summary("Sets the game of StatusBot. Creator only")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        public async Task SetPresence(ActivityType type, [Remainder] string game = null)
        {
            var client = Context.Client;
            await client.SetGameAsync(game, type: type);
            if (game == null) await Context.Channel.SendMessageAsync("Successfully reset current presence");
            else await Context.Channel.SendMessageAsync($"Successfully set presence to {game}");
        }

        [Command("sql")]
        [RequireOwner]
        public async Task ExecuteSQL(string option, [Remainder] string query = null)
        {
            List<List<string>> records = await DA.ExecSql(query);
            if (option == "raw")
            {
                //string flatdata = String.Join("\n", records.Select(record => String.Join("\t", record)));
                string flatdata = "";
                foreach (var record in records)
                {
                    foreach (var field in record)
                    {
                        int spacecount = 20 - field.Length;
                        string space = "";
                        for (int i = 0; i < spacecount; i++)
                            space += " ";
                        flatdata += $"{field}{space}";
                    }
                    flatdata += '\n';
                }
                Console.WriteLine(flatdata);
                await ReplyAsync($"```\n{flatdata}\n```");
            }
            else if (option == "csv")
            {
                string csvstring = string.Join("\n", records.Select(record => string.Join(",", record)));
                var timestamp = DateTime.Now.ToLocalTime().ToFileTime();
                string filename = $"query_{timestamp}.csv";

                var MS = new MemoryStream();
                using (StreamWriter writer = new StreamWriter(MS))
                {
                    Console.WriteLine($"before write {MS.Length} pos: {MS.Position}");
                    await writer.WriteAsync(csvstring);
                    await writer.FlushAsync();
                    Console.WriteLine($"after write {MS.Length} pos: {MS.Position}");

                    MS.Position = 0;
                    Console.WriteLine($"after flush {MS.Length} pos: {MS.Position}");
                    using (StreamReader reader = new StreamReader(MS))
                    {
                        await Context.Channel.SendFileAsync(reader.BaseStream, filename);
                    }
                }
            }
        }

        [Command("timers", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task GetTimers()
        {
            if (!TS.Timers.Any()) { await ReplyAsync("No timers"); return; }
            await ReplyAsync(string.Join("\n", TS.Timers.Select(t => $"{t.Key} {t.Value}")));
        }

        [Command("update_docs", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task Inspect(string format, string path, [Remainder] string module){
            var commands = CS.Modules.FirstOrDefault(m => m.Name == module).Commands.Where(c => !string.IsNullOrWhiteSpace(c.Summary))
                            .OrderBy(x => x.Name).ToArray();
            if (format == "txt"){
                await ReplyAsync(string.Join("\n", commands.Select(c => $"**{c.Name}**\n{c.Summary}").ToList()));
            }
            else if (format == "rst"){
                StringBuilder rst = new StringBuilder();
                rst.Append($"*****************\n{module}\n*****************\n\n");
                await ReplyAsync($"Obtained {commands.Count()}");
                foreach (var cmd in commands){
                    Console.WriteLine($"Command: {cmd.Name}");
                    rst.Append($"{cmd.Name}\n---------------\n");
                    string summary = cmd.Summary ?? "None";
                    // if (summary.Contains("Usage:")){
                    //     rst.Append(".. parsed-literal::\n");
                    //     rst.Append($"    |bot_prefix|\\ {cmd.Name}\n");
                    // }
                    var summarray = summary.Split('\n');
                    foreach (string sumline in summarray){
                        if (sumline.StartsWith("Usage:")){  // Syntax in the second line of the command's summary. If exists, placed first after command name
                            string syntax = sumline.Substring(6).Trim().Replace("`", "");
                            rst.Append(".. code::\n\n");
                            rst.Append($"\t{syntax}\n\n");
                        }
                        else if (sumline.StartsWith("Example:")){
                            string examplestr = sumline.Substring(8).Trim();  // Cut off the 'Example:' part of the string
                            int lasttick = examplestr.LastIndexOf("`");
                            string code = examplestr.Substring(1, lasttick - 1);
                            string details = examplestr.Substring(lasttick + 1).Trim();
                            rst.Append($"Example:\n\n``{code}`` {details}\n");
                        }
                        else if (sumline.Trim() == "Examples:"){
                            rst.Append($"Examples:\n\n");
                        }
                        else if (sumline.StartsWith("`")){  // Example commands
                            int lasttick = sumline.LastIndexOf("`");
                            string code = sumline.Substring(1, lasttick - 1);
                            string details = sumline.Substring(lasttick + 1).Trim();
                            rst.Append($"- ``{code}``\n  {details}\n\n");
                        }
                        else{
                            rst.Append($"{sumline.Replace("`", "``")}\n\n");
                        }
                    }
                    if (cmd != commands.Last())
                        rst.Append("\n....\n\n");
                }
                string rstring = rst.ToString();
                if (rstring.Count() > 2000){
                    foreach (int page in Enumerable.Range(1, (int)Math.Ceiling((double)rstring.Count() / 2000))){
                        int skip = ((page - 1) * 2000);
                        Console.WriteLine(skip);
                        await ReplyAsync(new String(rstring.Skip(skip).Take(2000).ToArray()));
                    }
                }
                else
                    await ReplyAsync(rstring);
                //MemoryStream MS = new MemoryStream();
                using (StreamWriter writer = new StreamWriter($@"{path}.rst"))
                {
                    await writer.WriteLineAsync(rstring);
                    await writer.FlushAsync();
                }
            }
        }
    }
}
