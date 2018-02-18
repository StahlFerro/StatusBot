﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatusBot.Models
{
    public class LISTENER
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ListenerID { get; set; }
        public ulong UserID { get; set; }

        [ForeignKey("ReminderIDFK")]
        public REMINDERCONFIG ReminderConfig { get; set; }
        public ulong ReminderIDFK { get; set; }
    }
}
