using LocalCommunityBoard.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace LocalCommunityBoard.Domain.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("username")]
        public string Username { get; set; } = null!;

        [Required]
        [Column("email")]
        public string Email { get; set; } = null!;

        [Required]
        [Column("password")]
        public string Password { get; set; } = null!;

        [Column("role_id")]
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        [Column("status")]
        public int Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навігація
        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
  