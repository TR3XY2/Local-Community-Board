// <copyright file="AnnouncementRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Infrastructure.Repositories;

using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Interfaces;
using LocalCommunityBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repository for managing announcements.
/// </summary>
public class AnnouncementRepository : Repository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(LocalCommunityBoardDbContext context)
        : base(context)
    {
    }

    public async Task<(IEnumerable<Announcement> Items, int TotalCount)> GetFilteredPagedAsync(
        string? city,
        string? district,
        string? street,
        IEnumerable<int>? categoryIds,
        DateTime? date,
        int pageNumber,
        int pageSize)
    {
        var query = this.DbSet
            .Include(a => a.User)
            .Include(a => a.Category)
            .Include(a => a.Location)
            .AsQueryable();

        // --- Location filters ---
        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(a =>
                a.Location.City != null &&
                a.Location.City.Contains(city, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(district))
        {
            query = query.Where(a =>
                a.Location.District != null &&
                a.Location.District.Contains(district, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(street))
        {
            query = query.Where(a =>
                a.Location.Street != null &&
                a.Location.Street.Contains(street, StringComparison.OrdinalIgnoreCase));
        }

        // --- Categories ---
        if (categoryIds?.Any() == true)
        {
            query = query.Where(a => categoryIds.Contains(a.CategoryId));
        }

        // --- Date ---
        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(a => a.CreatedAt >= start && a.CreatedAt < end);
        }

        // --- Count before paging ---
        var totalCount = await query.CountAsync();

        // --- Apply paging ---
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Announcement>> GetByUserIdAsync(int userId)
    {
        return await this.DbSet
            .Include(a => a.Category)
            .Include(a => a.Location)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAnnouncementAsync(
        int announcementId,
        int userId,
        string? title = null,
        string? body = null,
        int? categoryId = null)
    {
        var announcement = await this.DbSet.FirstOrDefaultAsync(a => a.Id == announcementId && a.UserId == userId);
        if (announcement == null)
        {
            return false; // Announcement not found or user not authorized
        }

        // Update fields if new values are provided
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
            announcement.CategoryId = categoryId.Value;
        }

        this.DbSet.Update(announcement);
        await this.SaveChangesAsync();
        return true;
    }
}
