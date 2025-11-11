// <copyright file="IReactionService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Interfaces;

using System.Threading.Tasks;

using LocalCommunityBoard.Domain.Entities;

/// <summary>
/// Service abstraction for managing reactions (likes) on announcements.
/// </summary>
public interface IReactionService
{
    /// <summary>
    /// Toggles a like for a user on a specific announcement.
    /// </summary>
    /// <param name="announcementId">The announcement identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns><c>true</c> if the announcement is now liked; otherwise <c>false</c>.</returns>
    Task<bool> ToggleLikeAsync(int announcementId, int userId);

    /// <summary>
    /// Gets the total count of likes for the specified announcement.
    /// </summary>
    /// <param name="announcementId">The announcement identifier.</param>
    /// <returns>The number of likes.</returns>
    Task<int> GetLikesCountAsync(int announcementId);

    /// <summary>
    /// Determines whether the given user has liked the specified announcement.
    /// </summary>
    /// <param name="announcementId">The announcement identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns><c>true</c> if the user liked it; otherwise <c>false</c>.</returns>
    Task<bool> HasUserLikedAsync(int announcementId, int userId);

    Task<bool> ToggleDislikeAsync(int announcementId, int userId);

    Task<int> GetDislikesCountAsync(int announcementId);

    Task<bool> HasUserDislikedAsync(int announcementId, int userId);
}
