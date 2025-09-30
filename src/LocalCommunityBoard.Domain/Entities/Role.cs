using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalCommunityBoard.Domain.Entities
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        // Навігація
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
