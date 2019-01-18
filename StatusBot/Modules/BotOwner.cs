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
        private CommandService C;
        public BotOwner(IServiceProvider ISP)
        {
            DA = ISP.GetService<DataService>();
            TS = ISP.GetService<TimerService>();
            R = new Random();
            C = ISP.GetService<CommandService>();
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

        [Command("inspect_module", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task Inspect(string format, [Remainder] string module){
            var commands = C.Modules.FirstOrDefault(m => m.Name == module).Commands.OrderBy(x => x.Name);
            if (format == "txt"){
                await ReplyAsync(string.Join("\n", commands.Select(c => $"**{c.Name}**\n{c.Summary}").ToList()));
            }
            else if (format == "rst"){
                StringBuilder rst = new StringBuilder();
                foreach (var cmd in commands){
                    rst.Append($"{cmd.Name}\n---------------\n");
                    string summary = cmd.Summary;
                    if (!summary.Contains("Usage:")){
                        rst.Append(".. parsed-literal::\n");
                        rst.Append($"    |bot_prefix|\\ {cmd.Name}\n");
                    }
                    var summarray = summary.Split('\n');
                    foreach (string sumline in summarray){
                        if (sumline == summarray[0]){
                            rst.Append($"{sumline}\n");
                        }
                        else if (sumline.StartsWith("Example:")){
                            string singlehelpstr = sumline.Substring(8).TrimStart();
                            rst.Append($"Example:\n{singlehelpstr}\n");
                        }
                        else if (sumline.Trim() == "Examples:"){
                            rst.Append($"Examples:\n");
                        }
                        else if (sumline.StartsWith("`")){
                            rst.Append($"{sumline}\n");
                        }
                    }
                    rst.Append("....\n\n");
                    string desc = summary.IndexOf("Summary: ").ToString();
                }
                await ReplyAsync(rst.ToString());
                MemoryStream MS = new MemoryStream();
            }
        }
    }
}
