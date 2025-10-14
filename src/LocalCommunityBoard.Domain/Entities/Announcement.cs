// <copyright file="Announcement.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LocalCommunityBoard.Domain.Enums;

/// <summary>
/// Represents an announcement entity in the system.
/// </summary>
[Table("Announcements")]
public class Announcement
{
    /// <summary>
    /// Gets or sets the unique identifier for the announcement.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the announcement.
    /// </summary>
    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user who created the announcement.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the category associated with the announcement.
    /// </summary>
    [Column("category_id")]
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the category associated with the announcement.
    /// </summary>
    public Category Category { get; set; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the location associated with the announcement.
    /// </summary>
    [Column("location_id")]
    public int LocationId { get; set; }

    /// <summary>
    /// Gets or sets the location associated with the announcement.
    /// </summary>
    public Location Location { get; set; } = null!;

    /// <summary>
    /// Gets or sets the title of the announcement.
    /// </summary>
    [Required]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the body content of the announcement.
    /// </summary>
    [Required]
    [Column("body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of the announcement.
    /// </summary>
    [Column("status")]
    public AnnouncementStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the announcement was created.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets an optional image URL associated with the announcement.
    /// </summary>
    [Column("image_url")]
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the collection of comments associated with the announcement.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Gets or sets the collection of reactions associated with the announcement.
    /// </summary>
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
}
