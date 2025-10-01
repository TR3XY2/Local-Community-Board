// <copyright file="Subscription.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a subscription entity that links a user to a location.
/// </summary>
[Table("Subscriptions")]
public class Subscription
{
    /// <summary>
    /// Gets or sets the unique identifier for the subscription.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the subscription.
    /// </summary>
    [Column("user_id")]
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user associated with the subscription.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier of the location associated with the subscription.
    /// </summary>
    [Column("location_id")]
    [Required]
    public int LocationId { get; set; }

    /// <summary>
    /// Gets or sets the location associated with the subscription.
    /// </summary>
    public Location Location { get; set; } = null!;

    /// <summary>
    /// Gets or sets the date and time when the subscription was created.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
