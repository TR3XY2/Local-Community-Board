// <copyright file="AnnouncementStatus.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents the status of an announcement.
/// </summary>
public enum AnnouncementStatus
{
    /// <summary>
    /// The announcement is active and visible.
    /// </summary>
    Active,

    /// <summary>
    /// The announcement has been deleted.
    /// </summary>
    Deleted,

    /// <summary>
    /// The announcement is under moderation.
    /// </summary>
    Moderated,

    /// <summary>
    /// The announcement has expired.
    /// </summary>
    Expired,
}
