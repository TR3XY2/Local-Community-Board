using LocalCommunityBoard.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace LocalCommunityBoard.Domain.Entities
{
    public class ModerationAction
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public int AdminId { get; set; }
        public User Admin { get; set; } = null!;

        [Required] 
        public TargetType TargetType { get; set; }

        [Required] 
        public int TargetId { get; set; }

        [Required] 
        public ModerationActionType Action { get; set; }

        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}