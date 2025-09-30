using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalCommunityBoard.Domain.Entities
{
    public class Role
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
