// <copyright file="ModerationActionType.cs" company="PlaceholderCompany">
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
    /// Represents the types of moderation actions that can be performed.
    /// </summary>
    public enum ModerationActionType
    {
        /// <summary>
        /// Represents an action to edit content.
        /// </summary>
        Edit,

        /// <summary>
        /// Represents an action to delete content.
        /// </summary>
        Delete,

        /// <summary>
        /// Represents an action to block a user or content.
        /// </summary>
        Block,

        /// <summary>
        /// Represents an action to unblock a user or content.
        /// </summary>
        Unblock,
    }
}
