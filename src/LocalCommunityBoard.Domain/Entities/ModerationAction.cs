// <copyright file="ModerationAction.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using LocalCommunityBoard.Domain.Enums;

    [Table("ModerationActions")]
    public class ModerationAction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("admin_id")]
        public int AdminId { get; set; }

        public User Admin { get; set; } = null!;

        [Required]
        [Column("target_type")]
        public TargetType TargetType { get; set; }

        [Required]
        [Column("target_id")]
        public int TargetId { get; set; }

        [Column("action")]
        public ModerationActionType Action { get; set; }

        [Column("reason")]
        public string? Reason { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
