// <copyright file="Location.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents a location entity with details such as city, district, street, and house.
/// </summary>
[Table("Locations")]
public class Location
{
    /// <summary>
    /// Gets or sets the unique identifier for the location.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the city of the location. This field is required.
    /// </summary>
    [Required]
    [Column("city")]
    public string City { get; set; } = null!;

    /// <summary>
    /// Gets or sets the district of the location. This field is optional.
    /// </summary>
    [Column("district")]
    public string? District { get; set; }

    /// <summary>
    /// Gets or sets the street of the location. This field is optional.
    /// </summary>
    [Column("street")]
    public string? Street { get; set; }

    /// <summary>
    /// Gets or sets the house number or name of the location. This field is optional.
    /// </summary>
    [Column("house")]
    public string? House { get; set; }

    /// <summary>
    /// Gets or sets the collection of announcements associated with this location.
    /// </summary>
    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    /// <summary>
    /// Gets or sets the collection of subscriptions associated with this location.
    /// </summary>
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
