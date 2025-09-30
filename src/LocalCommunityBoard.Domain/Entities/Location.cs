using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalCommunityBoard.Domain.Entities
{
    [Table("Locations")]
    public class Location
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("city")]
        public string City { get; set; } = null!;

        [Column("district")]
        public string? District { get; set; }

        [Column("street")]
        public string? Street { get; set; }

        [Column("house")]
        public string? House { get; set; }

        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
