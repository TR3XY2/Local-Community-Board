// <copyright file="ReportRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Infrastructure.Repositories;

using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using LocalCommunityBoard.Domain.Interfaces;
using LocalCommunityBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repository for managing reports.
/// </summary>
public class ReportRepository : Repository<Report>, IReportRepository
{
    public ReportRepository(LocalCommunityBoardDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Report>> GetByTargetAsync(TargetType targetType, int targetId)
    {
        return await this.DbSet
            .Include(r => r.Reporter)
            .Where(r => r.TargetType == targetType && r.TargetId == targetId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Report>> GetByReporterAsync(int reporterId)
    {
        return await this.DbSet
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Report>> GetByStatusAsync(ReportStatus status)
    {
        return await this.DbSet
            .Include(r => r.Reporter)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> HasUserReportedAsync(int reporterId, TargetType targetType, int targetId)
    {
        return await this.DbSet.AnyAsync(r =>
            r.ReporterId == reporterId &&
            r.TargetType == targetType &&
            r.TargetId == targetId);
    }
}
