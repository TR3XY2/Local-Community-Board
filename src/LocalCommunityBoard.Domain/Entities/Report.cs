// <copyright file="Report.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocalCommunityBoard.Domain.Enums;

/// <summary>
/// Represents a report entity in the system.
/// </summary>
[Table("Reports")]
public class Report
{
    /// <summary>
    /// Gets or sets the unique identifier for the report.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the report.
    /// </summary>
    [Required]
    [Column("reporter_id")]
    public int ReporterId { get; set; }

    /// <summary>
    /// Gets or sets the user who created the report.
    /// </summary>
    public User Reporter { get; set; } = null!;

    /// <summary>
    /// Gets or sets the type of the target being reported.
    /// </summary>
    [Required]
    [Column("target_type")]
    public TargetType TargetType { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the target being reported.
    /// </summary>
    [Required]
    [Column("target_id")]
    public int TargetId { get; set; }

    /// <summary>
    /// Gets or sets the reason for the report.
    /// </summary>
    [Column("reason")]
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the status of the report.
    /// </summary>
    [Column("status")]
    public ReportStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the report was created.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
