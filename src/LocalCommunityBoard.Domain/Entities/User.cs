// <copyright file="User.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using LocalCommunityBoard.Domain.Enums;

/// <summary>
/// Represents a user entity in the system.
/// </summary>
[Table("Users")]
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    [Required]
    [Column("username")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [Required]
    [Column("email")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets phone number of the user.
    /// </summary>
    [Required]
    [Column("phone_number")]
    public string PhoneNumber { get; set; } = null!;

    /// <summary>
    /// Gets or sets the hashed password of the user.
    /// </summary>
    [Required]
    [Column("password")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// Gets or sets the role identifier associated with the user.
    /// </summary>
    [Column("role_id")]
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the role associated with the user.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the status of the user.
    /// </summary>
    [Column("status")]
    public UserStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was created.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of announcements created by the user.
    /// </summary>
    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    /// <summary>
    /// Gets or sets the collection of comments made by the user.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Gets or sets the collection of reactions made by the user.
    /// </summary>
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

    /// <summary>
    /// Gets or sets the collection of subscriptions associated with the user.
    /// </summary>
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    /// <summary>
    /// Gets or sets the collection of reports submitted by the user.
    /// </summary>
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
