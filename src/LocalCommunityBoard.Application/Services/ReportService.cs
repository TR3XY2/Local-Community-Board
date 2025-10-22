// <copyright file="ReportService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Services;

using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using LocalCommunityBoard.Domain.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides business logic for managing reports.
/// </summary>
public class ReportService : IReportService
{
    private readonly IReportRepository reportRepository;
    private readonly ICommentRepository commentRepository;
    private readonly ILogger<ReportService> logger;

    public ReportService(
        IReportRepository reportRepository,
        ICommentRepository commentRepository,
        ILogger<ReportService> logger)
    {
        this.reportRepository = reportRepository;
        this.commentRepository = commentRepository;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Report> ReportCommentAsync(int reporterId, int commentId, string reason)
    {
        // Validate comment exists
        var comment = await this.commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            this.logger.LogWarning("Attempted to report non-existent comment ID {CommentId}", commentId);
            throw new ArgumentException($"Comment with ID {commentId} does not exist.");
        }

        // Check if user already reported this comment
        if (await this.reportRepository.HasUserReportedAsync(reporterId, TargetType.Comment, commentId))
        {
            this.logger.LogWarning("User {ReporterId} attempted to report comment {CommentId} multiple times", reporterId, commentId);
            throw new InvalidOperationException("You have already reported this comment.");
        }

        // Check that you are not reporting your own comment
        if (comment.UserId == reporterId)
        {
            this.logger.LogWarning("User {ReporterId} attempted to report their own comment {CommentId}", reporterId, commentId);
            throw new InvalidOperationException("You cannot report your own comment.");
        }

        var report = new Report
        {
            ReporterId = reporterId,
            TargetType = TargetType.Comment,
            TargetId = commentId,
            Reason = reason?.Trim(),
            Status = ReportStatus.Open,
            CreatedAt = DateTime.UtcNow,
        };

        await this.reportRepository.AddAsync(report);
        await this.reportRepository.SaveChangesAsync();

        this.logger.LogInformation("User {ReporterId} reported comment {CommentId}", reporterId, commentId);
        return report;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Report>> GetReportsForTargetAsync(TargetType targetType, int targetId)
    {
        return await this.reportRepository.GetByTargetAsync(targetType, targetId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Report>> GetReportsByStatusAsync(ReportStatus status)
    {
        return await this.reportRepository.GetByStatusAsync(status);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateReportStatusAsync(int reportId, ReportStatus status)
    {
        var report = await this.reportRepository.GetByIdAsync(reportId);
        if (report == null)
        {
            return false;
        }

        report.Status = status;
        this.reportRepository.Update(report);
        await this.reportRepository.SaveChangesAsync();

        this.logger.LogInformation("Report {ReportId} status updated to {Status}", reportId, status);
        return true;
    }
}
