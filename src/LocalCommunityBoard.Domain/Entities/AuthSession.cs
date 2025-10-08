// <copyright file="AuthSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents an authentication session.
/// </summary>
[Table("AuthSessions")]
public class AuthSession
{
    /// <summary>Gets or sets the session identifier (GUID).</summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>Gets or sets the user identifier.</summary>
    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the session was created.</summary>
    [Column("created_utc")]
    public DateTime CreatedUtc { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the session was invalidated (logout).</summary>
    [Column("revoked_utc")]
    public DateTime? RevokedUtc { get; set; }

    /// <summary>Gets or sets a value indicating whether the session is revoked.</summary>
    [Column("is_revoked")]
    public bool IsRevoked { get; set; }
}
