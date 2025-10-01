// <copyright file="TargetType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Enums
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the type of target in the system.
    /// </summary>
    public enum TargetType
    {
        /// <summary>
        /// Represents an announcement target.
        /// </summary>
        Announcement,

        /// <summary>
        /// Represents a comment target.
        /// </summary>
        Comment,

        /// <summary>
        /// Represents a user target.
        /// </summary>
        User,
    }
}
