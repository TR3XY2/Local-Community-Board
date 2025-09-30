using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalCommunityBoard.Domain.Entities
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        // Навігація
        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    }
}
