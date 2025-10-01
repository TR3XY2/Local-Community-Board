// <copyright file="Reaction.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using LocalCommunityBoard.Domain.Enums;

    /// <summary>
    /// Represents a reaction entity that associates a user with a specific announcement and reaction type.
    /// </summary>
    [Table("Reactions")]
    public class Reaction
    {
        /// <summary>
        /// Gets or sets the unique identifier for the reaction.
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
        /// Gets or sets the associated announcement entity.
        /// </summary>
        public Announcement Announcement { get; set; } = null!;

        /// <summary>
        /// Gets or sets the identifier of the user who made the reaction.
        /// </summary>
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the associated user entity.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the type of the reaction.
        /// </summary>
        [Required]
        [Column("type")]
        public ReactionType Type { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the reaction was created.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
