using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace StatusBot.Services.ReminderService
{
    public class ReminderConfig
    {
        public string GuildName { get; set; }
        public bool Active { get; set; }
        public ulong TargetID { get; set; }
        public string TargetName { get; set; }
        public List<ulong> ReceiverID { get; set; }
    }

    public class ReminderService
    {
        public string bookfilepath = "reminderbook.json";

        public async Task CreateNewRegistry(SocketGuild G)
        {
            ReminderConfig config = new ReminderConfig();
            config.GuildName = G.Name;
            config.Active = false;
            config.ReceiverID = new List<ulong>();
            Dictionary<ulong?, ReminderConfig> configbook;
            string oldjson = File.ReadAllText(bookfilepath);
            if (!String.IsNullOrWhiteSpace(oldjson))
                configbook = JsonConvert.DeserializeObject<Dictionary<ulong?, ReminderConfig>>(oldjson);
            else
                configbook = new Dictionary<ulong?, ReminderConfig>();
            if (!configbook.Keys.Contains(G.Id)) configbook.Add(G.Id, config);
            else return;
            string newjson = JsonConvert.SerializeObject(configbook, Formatting.Indented);
            File.WriteAllText(bookfilepath, newjson);
            await Task.CompletedTask;
        }

        public Dictionary<ulong?, ReminderConfig> GetBook()
        {
            string json = File.ReadAllText(bookfilepath);
            return JsonConvert.DeserializeObject<Dictionary<ulong?, ReminderConfig>>(json);
        }

        public KeyValuePair<ulong?, ReminderConfig> GetGuildConfig(SocketGuild G)
        {
            return GetBook().FirstOrDefault(x => x.Key == G.Id);
        }

        public async Task ModifyConfig(SocketGuild G, int a, SocketGuildUser target, List<ulong> receivers, string operation)
        {
            Dictionary<ulong?, ReminderConfig> configbook = GetBook();
            KeyValuePair<ulong?, ReminderConfig> oldconfig = GetGuildConfig(G);
            ReminderConfig newconfig = oldconfig.Value;

            if (a == -1) newconfig.Active = false;
            if (a == 0) newconfig.Active = oldconfig.Value.Active;
            if (a == 1) newconfig.Active = true;

            if (target != null)
            {
                newconfig.TargetID = target.Id;
                newconfig.TargetName = target.ToString();
            }

            if (receivers != null)
            {
                if (operation == "Add") foreach (var r in receivers) { newconfig.ReceiverID.Add(r); }
                if (operation == "Rem") foreach (var r in receivers) { newconfig.ReceiverID.Remove(r); }
                if (operation == "Clr") newconfig.ReceiverID.Clear();
            }

            configbook[G.Id] = newconfig;

            string newjson = JsonConvert.SerializeObject(configbook, Formatting.Indented);
            File.WriteAllText(bookfilepath, newjson);
            await Task.CompletedTask;
        }
        
        public async Task AutoPM(SocketGuildUser U, SocketGuildUser R)
        {
            var G = U.Guild;
            KeyValuePair<ulong?, ReminderConfig> guildconfig = GetGuildConfig(G);
            if (guildconfig.Value.TargetID == R.Id && guildconfig.Value.Active && R.Status == UserStatus.Offline && U.IsBot)
            {
                foreach (var rid in guildconfig.Value.ReceiverID)
                {
                    var receiver = Program.client.GetUser(rid);
                    var ch = await receiver.GetOrCreateDMChannelAsync();
                    await ch.SendMessageAsync($"{R} is offline");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{DateTime.Now.ToLocalTime()} Successfully PM'd {receiver} that {R} is offline");
                    File.AppendAllText("logfile.txt", $"{DateTime.Now.ToLocalTime()} Successfully PM'd {receiver} that {R} is offline\n");
                    Console.ResetColor();
                }
            }
        }
    }
}
