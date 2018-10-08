using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatusBot.Models
{
    public class Reminder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong ReminderId { get; set; }
        public ulong GuildId { get; set; }
        public ulong BotId { get; set; }
        public bool Active { get; set; }
        public int Duration { get; set; }
        public ICollection<Listener> Listeners { get; set; }
    }
}
