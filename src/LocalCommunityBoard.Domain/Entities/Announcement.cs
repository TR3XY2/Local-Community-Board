using LocalCommunityBoard.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalCommunityBoard.Domain.Entities
{
    [Table("Announcements")]
    public class Announcement
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Column("category_id")]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [Column("location_id")]
        public int LocationId { get; set; }
        public Location Location { get; set; } = null!;

        [Required]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("body")]
        public string Body { get; set; } = string.Empty;

        [Column("status")]
        public AnnouncementStatus Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}