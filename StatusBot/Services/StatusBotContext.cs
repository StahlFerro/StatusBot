using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StatusBot.Models;

namespace StatusBot.Services
{
    public class StatusBotContext : DbContext
    {
        public DbSet<REMINDERCONFIG> REMINDERCONFIGs { get; set; }
        public DbSet<LISTENER> LISTENERs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=user\sqlexpress;Database=StatusBot;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }
}
