using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StatusBot.Models;

namespace StatusBot.Models
{
    public class StatusBotContext : DbContext
    {
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Listener> Listeners { get; set; }
        public DbSet<BotConfig> BotConfigs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Database/StatusBot.db");
        }
    }
}
