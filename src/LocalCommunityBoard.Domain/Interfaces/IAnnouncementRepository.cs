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
    /// <returns>A collection of filtered announcements.</returns>
    Task<IEnumerable<Announcement>> GetFilteredAsync(
        string? city,
        string? district,
        string? street,
        IEnumerable<int>? categoryIds,
        DateTime? date);

    /// <summary>
    /// Retrieves announcements created by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A collection of announcements created by the user.</returns>
    Task<IEnumerable<Announcement>> GetByUserIdAsync(int userId);
}
