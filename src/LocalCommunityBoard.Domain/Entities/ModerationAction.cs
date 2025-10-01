// <copyright file="ModerationAction.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocalCommunityBoard.Domain.Enums;

/// <summary>
/// Represents an action taken by an administrator to moderate content or users.
/// </summary>
[Table("ModerationActions")]
public class ModerationAction
{
    /// <summary>
    /// Gets or sets the unique identifier for the moderation action.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the administrator who performed the action.
    /// </summary>
    [Required]
    [Column("admin_id")]
    public int AdminId { get; set; }

    /// <summary>
    /// Gets or sets the administrator who performed the action.
    /// </summary>
    public User Admin { get; set; } = null!;

    /// <summary>
    /// Gets or sets the type of the target being moderated (e.g., user, post, etc.).
    /// </summary>
    [Required]
    [Column("target_type")]
    public TargetType TargetType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the target being moderated.
    /// </summary>
    [Required]
    [Column("target_id")]
    public int TargetId { get; set; }

    /// <summary>
    /// Gets or sets the type of moderation action performed (e.g., ban, warn, etc.).
    /// </summary>
    [Column("action")]
    public ModerationActionType Action { get; set; }

    /// <summary>
    /// Gets or sets the reason for the moderation action.
    /// </summary>
    [Column("reason")]
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the moderation action was created.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
