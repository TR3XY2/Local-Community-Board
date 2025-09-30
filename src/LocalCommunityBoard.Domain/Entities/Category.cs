using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalCommunityBoard.Domain.Entities
{
    public class Category
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public string Name { get; set; } = string.Empty;

        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    }
}
