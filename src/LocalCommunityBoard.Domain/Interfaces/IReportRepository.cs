// <copyright file="IReportRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Interfaces;

using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;

/// <summary>
/// Defines repository operations specific to reports.
/// </summary>
public interface IReportRepository : IRepository<Report>
{
    /// <summary>
    /// Retrieves all reports for a specific target.
    /// </summary>
    /// <param name="targetType">The type of target (Announcement, Comment, User).</param>
    /// <param name="targetId">The ID of the target.</param>
    /// <returns>A collection of reports for the target.</returns>
    Task<IEnumerable<Report>> GetByTargetAsync(TargetType targetType, int targetId);

    /// <summary>
    /// Retrieves all reports submitted by a specific user.
    /// </summary>
    /// <param name="reporterId">The ID of the reporter.</param>
    /// <returns>A collection of reports submitted by the user.</returns>
    Task<IEnumerable<Report>> GetByReporterAsync(int reporterId);

    /// <summary>
    /// Retrieves all reports with a specific status.
    /// </summary>
    /// <param name="status">The report status to filter by.</param>
    /// <returns>A collection of reports with the specified status.</returns>
    Task<IEnumerable<Report>> GetByStatusAsync(ReportStatus status);

    /// <summary>
    /// Checks if a user has already reported a specific target.
    /// </summary>
    /// <param name="reporterId">The ID of the reporter.</param>
    /// <param name="targetType">The type of target.</param>
    /// <param name="targetId">The ID of the target.</param>
    /// <returns>True if the user has already reported this target; otherwise false.</returns>
    Task<bool> HasUserReportedAsync(int reporterId, TargetType targetType, int targetId);
}
