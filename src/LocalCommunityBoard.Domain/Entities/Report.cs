using LocalCommunityBoard.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace LocalCommunityBoard.Domain.Entities
{
    public class Report
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public int ReporterId { get; set; }
        public User Reporter { get; set; } = null!;

        [Required] 
        public TargetType TargetType { get; set; }

        [Required] 
        public int TargetId { get; set; }

        public string? Reason { get; set; }

        public ReportStatus Status { get; set; } = ReportStatus.Open;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}