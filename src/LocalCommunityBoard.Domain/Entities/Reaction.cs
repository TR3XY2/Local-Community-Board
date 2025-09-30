using LocalCommunityBoard.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalCommunityBoard.Domain.Entities
{
    [Table("Reactions")]
    public class Reaction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("announcement_id")]
        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; } = null!;

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        [Column("type")]
        public ReactionType Type { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}