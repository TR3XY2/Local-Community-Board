using System.ComponentModel.DataAnnotations;

namespace LocalCommunityBoard.Domain.Entities
{
    public class Comment
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public int AnnouncementId { get; set; }
        public Announcement Announcement { get; set; } = null!;

        [Required] 
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }

        [Required] 
        public string Body { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}