// <copyright file="AnnouncementService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Services;

using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides business logic for managing announcements.
/// </summary>
public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository announcementRepository;
    private readonly IRepository<Location> locationRepository;
    private readonly IRepository<Category> categoryRepository;
    private readonly ILogger<AnnouncementService> logger;

    public AnnouncementService(
        IAnnouncementRepository announcementRepository,
        IRepository<Location> locationRepository,
        IRepository<Category> categoryRepository,
        ILogger<AnnouncementService> logger)
    {
        this.announcementRepository = announcementRepository;
        this.locationRepository = locationRepository;
        this.categoryRepository = categoryRepository;
        this.logger = logger;
    }

    public async Task<(IEnumerable<Announcement> Items, int TotalCount)> GetAnnouncementsPagedAsync(
        string? city = null,
        string? district = null,
        string? street = null,
        IEnumerable<int>? categoryIds = null,
        DateTime? date = null,
        int pageNumber = 1,
        int pageSize = 9)
    {
        this.logger.LogInformation("Fetching announcements (Page: {PageNumber}, Size: {PageSize})", pageNumber, pageSize);
        return await this.announcementRepository.GetFilteredPagedAsync(
            city, district, street, categoryIds, date, pageNumber, pageSize);
    }

    /// <inheritdoc/>
    public async Task<Announcement> CreateAnnouncementAsync(
        int userId,
        string title,
        string body,
        int categoryId,
        int locationId,
        IEnumerable<string>? imageUrls = null,
        IEnumerable<string>? links = null)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(title))
        {
            this.logger.LogWarning("User {UserId} attempted to create an announcement with empty title", userId);
            throw new ArgumentException("Title cannot be empty.");
        }

        // Validate category and location existence (without storing them)
        if (await this.categoryRepository.GetByIdAsync(categoryId) == null)
        {
            this.logger.LogWarning("User {UserId} attempted to use non-existent category ID {CategoryId}", userId, categoryId);
            throw new ArgumentException($"Category with ID {categoryId} does not exist.");
        }

        if (await this.locationRepository.GetByIdAsync(locationId) == null)
        {
            this.logger.LogWarning("User {UserId} attempted to use non-existent location ID {LocationId}", userId, locationId);
            throw new ArgumentException($"Location with ID {locationId} does not exist.");
        }

        var announcement = new Announcement
        {
            UserId = userId,
            Title = title.Trim(),
            Body = body.Trim(),
            CategoryId = categoryId,
            LocationId = locationId,
            CreatedAt = DateTime.UtcNow,
        };

        await this.announcementRepository.AddAsync(announcement);
        await this.announcementRepository.SaveChangesAsync();

        this.logger.LogInformation("User {UserId} created announcement {AnnouncementId}", userId, announcement.Id);
        return announcement;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAnnouncementAsync(
        int announcementId,
        int userId,
        string? title = null,
        string? body = null,
        int? categoryId = null,
        string? imageUrl = null)
    {
        var announcement = await this.announcementRepository.GetByIdAsync(announcementId);
        if (announcement == null)
        {
            this.logger.LogWarning("Attempted to update non-existent announcement ID {AnnouncementId}", announcementId);
            return false;
        }

        if (announcement.UserId != userId)
        {
            this.logger.LogWarning("User {UserId} attempted to update another user's announcement {AnnouncementId}", userId, announcementId);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            announcement.Title = title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(body))
        {
            announcement.Body = body.Trim();
        }

        if (categoryId.HasValue)
        {
            if (await this.categoryRepository.GetByIdAsync(categoryId.Value) == null)
            {
                this.logger.LogWarning("User {UserId} attempted to update announcement {AnnouncementId} with non-existent category {CategoryId}", userId, announcementId, categoryId);
                throw new ArgumentException($"Category with ID {categoryId.Value} does not exist.");
            }

            announcement.CategoryId = categoryId.Value;
        }

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            announcement.ImageUrl = imageUrl.Trim();
        }

        this.announcementRepository.Update(announcement);
        await this.announcementRepository.SaveChangesAsync();

        this.logger.LogInformation("User {UserId} updated announcement {AnnouncementId}", userId, announcementId);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAnnouncementAsync(int announcementId, int userId)
    {
        var announcement = await this.announcementRepository.GetByIdAsync(announcementId);
        if (announcement == null)
        {
            this.logger.LogWarning("Attempted to delete non-existent announcement ID {AnnouncementId}", announcementId);
            return false;
        }

        if (announcement.UserId != userId)
        {
            this.logger.LogWarning("User {UserId} attempted to delete another user's announcement {AnnouncementId}", userId, announcementId);
            return false;
        }

        this.announcementRepository.Delete(announcement);
        await this.announcementRepository.SaveChangesAsync();

        this.logger.LogInformation("User {UserId} deleted announcement {AnnouncementId}", userId, announcementId);
        return true;
    }

    public async Task<IEnumerable<Announcement>> GetAnnouncementsByUserIdAsync(int userId)
    {
        this.logger.LogInformation("Fetching announcements for user ID {UserId}", userId);
        return await this.announcementRepository.GetByUserIdAsync(userId);
    }
}
