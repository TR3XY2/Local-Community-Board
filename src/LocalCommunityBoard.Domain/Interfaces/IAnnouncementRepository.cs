// <copyright file="IAnnouncementRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Interfaces;

using LocalCommunityBoard.Domain.Entities;

/// <summary>
/// Defines repository operations specific to announcements.
/// </summary>
public interface IAnnouncementRepository : IRepository<Announcement>
{
    /// <summary>
    /// Retrieves announcements with optional filters for category, location, or date.
    /// </summary>
    /// <param name="city">Optional city name to filter by.</param>
    /// <param name="district">Optional district name to filter by.</param>
    /// <param name="street">Optional street name to filter by.</param>
    /// <param name="categoryIds">Optional list of category IDs to filter by.</param>
    /// <param name="date">Optional date to filter by (CreatedAt).</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A collection of filtered announcements.</returns>
    Task<(IEnumerable<Announcement> Items, int TotalCount)> GetFilteredPagedAsync(
        string? city,
        string? district,
        string? street,
        IEnumerable<int>? categoryIds,
        DateTime? date,
        int pageNumber,
        int pageSize);

    /// <summary>
    /// Retrieves announcements created by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A collection of announcements created by the user.</returns>
    Task<IEnumerable<Announcement>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Updates an existing announcement.
    /// </summary>
    /// <param name="announcementId">The ID of the announcement to update.</param>
    /// <param name="userId">The ID of the user attempting the update (for
    /// authorization).</param>
    /// <param name="title">The new title (optional).</param>
    /// <param name="body">The new body (optional).</param>
    /// <param name="categoryId">The new category ID (optional).</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdateAnnouncementAsync(
    int announcementId,
    int userId,
    string? title = null,
    string? body = null,
    int? categoryId = null);
}
