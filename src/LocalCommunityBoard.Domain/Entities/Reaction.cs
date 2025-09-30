using LocalCommunityBoard.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace LocalCommunityBoard.Domain.Entities
{
    public class Reaction
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; } = null!;

        [Required] 
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required] 
        public ReactionType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}