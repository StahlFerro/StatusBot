using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatusBot.Models
{
    public class REMINDERCONFIG
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong ReminderID { get; set; }
        public ulong GuildID { get; set; }
        public ulong BotID { get; set; }
        public bool Active { get; set; }
        public List<LISTENER> ListenerList { get; set; }
    }
}
