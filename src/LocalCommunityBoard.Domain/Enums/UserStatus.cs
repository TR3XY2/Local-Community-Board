// <copyright file="UserStatus.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents the status of a user in the system.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// The user is active and has full access.
    /// </summary>
    Active,

    /// <summary>
    /// The user is blocked and cannot access the system.
    /// </summary>
    Blocked,
}
