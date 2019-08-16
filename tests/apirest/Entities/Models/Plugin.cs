using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("plugin")]
    public class Plugin
    {

        [Key]
        public string Name { get; set; }

        public int HearthBeat { get; set; }
        public int DateAdded { get; set; }
        public int Version { get; set; }
    }
}
