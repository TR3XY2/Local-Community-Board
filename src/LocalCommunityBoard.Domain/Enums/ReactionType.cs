// <copyright file="ReactionType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents the type of reaction that can be given.
/// </summary>
public enum ReactionType
{
    /// <summary>
    /// Represents a "Like" reaction.
    /// </summary>
    Like,

    /// <summary>
    /// Represents a "Dislike" reaction.
    /// </summary>
    Dislike,
}
