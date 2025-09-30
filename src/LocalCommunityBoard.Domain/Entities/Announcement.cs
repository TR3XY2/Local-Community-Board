using LocalCommunityBoard.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace LocalCommunityBoard.Domain.Entities
{
    public class Announcement
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required] 
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [Required] 
        public int LocationId { get; set; }
        public Location Location { get; set; } = null!;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required] 
        public string Body { get; set; } = string.Empty;

        public AnnouncementStatus Status { get; set; } = AnnouncementStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}