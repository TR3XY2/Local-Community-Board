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

    /// <inheritdoc/>
    public async Task<IEnumerable<Announcement>> GetFilteredAsync(
    string? city,
    string? district,
    string? street,
    IEnumerable<int>? categoryIds,
    DateTime? date)
    {
        var query = this.DbSet
            .Include(a => a.User)
            .Include(a => a.Category)
            .Include(a => a.Location)
            .AsQueryable();

        // --- Filter by location ---
        if (!string.IsNullOrWhiteSpace(city))
        {
            var cityLower = city.ToLower();
            query = query.Where(a =>
                a.Location.City != null &&
                a.Location.City.ToLower().Contains(cityLower));
        }

        if (!string.IsNullOrWhiteSpace(district))
        {
            var districtLower = district.ToLower();
            query = query.Where(a =>
                a.Location.District != null &&
                a.Location.District.ToLower().Contains(districtLower));
        }

        if (!string.IsNullOrWhiteSpace(street))
        {
            var streetLower = street.ToLower();
            query = query.Where(a =>
                a.Location.Street != null &&
                a.Location.Street.ToLower().Contains(streetLower));
        }

        // --- Filter by categories ---
        if (categoryIds?.Any() == true)
        {
            query = query.Where(a => categoryIds.Contains(a.CategoryId));
        }

        // --- Filter by date (same day) ---
        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(a => a.CreatedAt >= start && a.CreatedAt < end);
        }

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
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
}
