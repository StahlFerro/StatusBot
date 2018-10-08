using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatusBot.Models
{
    public class Listener
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ListenerId { get; set; }
        public ulong UserID { get; set; }

        [ForeignKey("ReminderId")]
        public Reminder Reminder { get; set; }
        public ulong ReminderId { get; set; }
    }
}
