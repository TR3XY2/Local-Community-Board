using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalCommunityBoard.Domain.Entities
{
    public class Location
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public string City { get; set; } = string.Empty;

        public string? District { get; set; }
        public string? Street { get; set; }
        public string? House { get; set; }

        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
