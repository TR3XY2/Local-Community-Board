// <copyright file="Role.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a role entity in the system.
    /// </summary>
    [Table("Roles")]
    public class Role
    {
        /// <summary>
        /// Gets or sets the unique identifier for the role.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the description of the role.
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the collection of users associated with the role.
        /// </summary>
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
