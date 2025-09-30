using LocalCommunityBoard.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace LocalCommunityBoard.Domain.Entities
{
    public class User
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public string Username { get; set; } = string.Empty;

        [Required] 
        public string Email { get; set; } = string.Empty;

        [Required] 
        public string Password { get; set; } = string.Empty;

        [Required] 
        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;

        public UserStatus Status { get; set; } = UserStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<ModerationAction> ModerationActions { get; set; } = new List<ModerationAction>();
    }
}