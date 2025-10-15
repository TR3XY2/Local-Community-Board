// <copyright file="IAnnouncementService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Interfaces;

using LocalCommunityBoard.Domain.Entities;

/// <summary>
/// Defines application-level operations related to announcements.
/// </summary>
public interface IAnnouncementService
{
    /// <summary>
    /// Retrieves announcements with optional filters for city, district, street, category, and date.
    /// </summary>
    Task<(IEnumerable<Announcement> Items, int TotalCount)> GetAnnouncementsPagedAsync(
        string? city = null,
        string? district = null,
        string? street = null,
        IEnumerable<int>? categoryIds = null,
        DateTime? date = null,
        int pageNumber = 1,
        int pageSize = 9);

    /// <summary>
    /// Creates a new announcement.
    /// </summary>
    Task<Announcement> CreateAnnouncementAsync(
        int userId,
        string title,
        string body,
        int categoryId,
        int locationId,
        IEnumerable<string>? imageUrls = null,
        IEnumerable<string>? links = null);

    /// <summary>
    /// Updates an existing announcement (only if owned by user).
    /// </summary>
    Task<bool> UpdateAnnouncementAsync(
        int announcementId,
        int userId,
        string? title = null,
        string? body = null,
        int? categoryId = null);

    /// <summary>
    /// Deletes an announcement (only if owned by user).
    /// </summary>
    Task<bool> DeleteAnnouncementAsync(int announcementId, int userId);

    /// <summary>
    /// Retrieves all announcements created by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A collection of announcements.</returns>
    Task<IEnumerable<Announcement>> GetAnnouncementsByUserIdAsync(int userId);
}
