using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatusBot.Models
{
    public class BotConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong BotID { get; set; }
        public ulong HeadquartersGuildID { get; set; }
        public string VersionNumber { get; set; }
        public string DefaultGame { get; set; }
        public string InviteUrl { get; set; }
    }
}
