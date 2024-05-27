﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TournamentAssistantServer.Database.Models
{
    [Table("Pools")]
    public class Pool
    {
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public ulong ID { get; set; }

        [Column("Guid")]
        public string Guid { get; set; }

        [Column("TournamentId")]
        public string TournamentId { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Image")]
        public string Image { get; set; }

        [Column("Old")]
        public bool Old { get; set; }
    }
}
