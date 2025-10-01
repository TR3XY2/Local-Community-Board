// <copyright file="Report.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using LocalCommunityBoard.Domain.Enums;

    [Table("Reports")]
    public class Report
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("reporter_id")]
        public int ReporterId { get; set; }

        public User Reporter { get; set; } = null!;

        [Required]
        [Column("target_type")]
        public TargetType TargetType { get; set; }

        [Required]
        [Column("target_id")]
        public int TargetId { get; set; }

        [Column("reason")]
        public string? Reason { get; set; }

        [Column("status")]
        public ReportStatus Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
