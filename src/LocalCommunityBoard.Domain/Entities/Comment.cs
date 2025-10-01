// <copyright file="Comment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a comment entity in the system.
/// </summary>
[Table("Comments")]
public class Comment
{
    /// <summary>
    /// Gets or sets the unique identifier for the comment.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated announcement.
    /// </summary>
    [Required]
    [Column("announcement_id")]
    public int AnnouncementId { get; set; }

    /// <summary>
    /// Gets or sets the associated announcement.
    /// </summary>
    public Announcement Announcement { get; set; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the user who created the comment.
    /// </summary>
    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user who created the comment.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the parent comment, if any.
    /// </summary>
    [Column("parent_comment_id")]
    public int? ParentCommentId { get; set; }

    /// <summary>
    /// Gets or sets the parent comment, if any.
    /// </summary>
    public Comment? ParentComment { get; set; }

    /// <summary>
    /// Gets or sets the body content of the comment.
    /// </summary>
    [Required]
    [Column("body")]
    public string Body { get; set; } = null!;

    /// <summary>
    /// Gets or sets the date and time when the comment was created.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of replies to this comment.
    /// </summary>
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
