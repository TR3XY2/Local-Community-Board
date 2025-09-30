using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalCommunityBoard.Domain.Entities
{
    [Table("Comments")]
    public class Comment
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

        [Column("parent_comment_id")]
        public int? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }

        [Required]
        [Column("body")]
        public string Body { get; set; } = null!;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}