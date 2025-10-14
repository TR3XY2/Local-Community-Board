// <copyright file="AnnouncementService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Services;

using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Interfaces;

/// <summary>
/// Provides business logic for managing announcements.
/// </summary>
public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository announcementRepository;
    private readonly IRepository<Location> locationRepository;
    private readonly IRepository<Category> categoryRepository;

    public AnnouncementService(
        IAnnouncementRepository announcementRepository,
        IRepository<Location> locationRepository,
        IRepository<Category> categoryRepository)
    {
        this.announcementRepository = announcementRepository;
        this.locationRepository = locationRepository;
        this.categoryRepository = categoryRepository;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Announcement>> GetAnnouncementsAsync(
        string? city = null,
        string? district = null,
        string? street = null,
        IEnumerable<int>? categoryIds = null,
        DateTime? date = null)
    {
        return await this.announcementRepository.GetFilteredAsync(city, district, street, categoryIds, date);
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
            throw new ArgumentException("Title cannot be empty.");
        }

        // Validate category and location existence (without storing them)
        if (await this.categoryRepository.GetByIdAsync(categoryId) == null)
        {
            throw new ArgumentException($"Category with ID {categoryId} does not exist.");
        }

        if (await this.locationRepository.GetByIdAsync(locationId) == null)
        {
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

        return announcement;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAnnouncementAsync(
        int announcementId,
        int userId,
        string? title = null,
        string? body = null,
        int? categoryId = null)
    {
        var announcement = await this.announcementRepository.GetByIdAsync(announcementId);
        if (announcement == null)
        {
            return false;
        }

        if (announcement.UserId != userId)
        {
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
                throw new ArgumentException($"Category with ID {categoryId.Value} does not exist.");
            }

            announcement.CategoryId = categoryId.Value;
        }

        this.announcementRepository.Update(announcement);
        await this.announcementRepository.SaveChangesAsync();

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAnnouncementAsync(int announcementId, int userId)
    {
        var announcement = await this.announcementRepository.GetByIdAsync(announcementId);
        if (announcement == null)
        {
            return false;
        }

        if (announcement.UserId != userId)
        {
            return false;
        }

        this.announcementRepository.Delete(announcement);
        await this.announcementRepository.SaveChangesAsync();

        return true;
    }
}
