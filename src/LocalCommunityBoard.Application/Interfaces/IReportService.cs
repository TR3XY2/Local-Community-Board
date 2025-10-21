// <copyright file="IReportService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Interfaces;

using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;

/// <summary>
/// Service abstraction for managing reports.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Creates a new report for a comment.
    /// </summary>
    /// <param name="reporterId">The ID of the user submitting the report.</param>
    /// <param name="commentId">The ID of the comment being reported.</param>
    /// <param name="reason">The reason for the report.</param>
    /// <returns>The created report.</returns>
    Task<Report> ReportCommentAsync(int reporterId, int commentId, string reason);

    /// <summary>
    /// Retrieves all reports for a specific target.
    /// </summary>
    /// <param name="targetType">The type of target.</param>
    /// <param name="targetId">The ID of the target.</param>
    /// <returns>A collection of reports.</returns>
    Task<IEnumerable<Report>> GetReportsForTargetAsync(TargetType targetType, int targetId);

    /// <summary>
    /// Retrieves all reports with a specific status.
    /// </summary>
    /// <param name="status">The report status.</param>
    /// <returns>A collection of reports.</returns>
    Task<IEnumerable<Report>> GetReportsByStatusAsync(ReportStatus status);

    /// <summary>
    /// Updates the status of a report.
    /// </summary>
    /// <param name="reportId">The ID of the report.</param>
    /// <param name="status">The new status.</param>
    /// <returns>True if successful; otherwise false.</returns>
    Task<bool> UpdateReportStatusAsync(int reportId, ReportStatus status);
}
