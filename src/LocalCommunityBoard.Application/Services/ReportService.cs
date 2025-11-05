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
    private readonly IAnnouncementRepository announcementRepository;
    private readonly IReportRepository reportRepository;
    private readonly ICommentRepository commentRepository;
    private readonly ILogger<ReportService> logger;

    public ReportService(
        IReportRepository reportRepository,
        ICommentRepository commentRepository,
        ILogger<ReportService> logger,
        IAnnouncementRepository announcementRepository)
    {
        this.reportRepository = reportRepository;
        this.commentRepository = commentRepository;
        this.logger = logger;
        this.announcementRepository = announcementRepository;
    }

    public async Task<bool> DeleteCommentByReportAsync(int reportId)
    {
        var report = await this.reportRepository.GetByIdAsync(reportId);
        if (report is null)
        {
            this.logger.LogWarning("DeleteCommentByReportAsync: report {ReportId} not found", reportId);
            return false;
        }

        if (report.TargetType != TargetType.Comment)
        {
            this.logger.LogWarning("DeleteCommentByReportAsync: report {ReportId} is not for a Comment", reportId);
            return false;
        }

        var commentId = report.TargetId;

        var comment = await this.commentRepository.GetByIdAsync(commentId);
        if (comment is null)
        {
            this.logger.LogWarning("DeleteCommentByReportAsync: comment {CommentId} not found", commentId);
            this.reportRepository.Delete(report);
            await this.reportRepository.SaveChangesAsync();
            return false;
        }

        var relatedReports = await this.reportRepository.GetByTargetAsync(TargetType.Comment, commentId);
        foreach (var r in relatedReports)
        {
            this.reportRepository.Delete(r);
        }

        this.commentRepository.Delete(comment);

        await this.reportRepository.SaveChangesAsync();
        await this.commentRepository.SaveChangesAsync();

        this.logger.LogInformation(
            "DeleteCommentByReportAsync: comment {CommentId} deleted by report {ReportId}", commentId, reportId);

        return true;
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

    public async Task<Report> ReportAnnouncementAsync(int reporterId, int postId, string reason)
    {
        var post = await this.announcementRepository.GetByIdAsync(postId);
        if (post == null)
        {
            this.logger.LogWarning("Attempted to report non-existent post ID {PostId}", postId);
            throw new ArgumentException($"Post with ID {postId} does not exist.");
        }

        if (await this.reportRepository.HasUserReportedAsync(reporterId, TargetType.Announcement, postId))
        {
            this.logger.LogWarning("User {ReporterId} attempted to report post {PostId} multiple times", reporterId, postId);
            throw new InvalidOperationException("You have already reported this post.");
        }

        var report = new Report
        {
            ReporterId = reporterId,
            TargetType = TargetType.Announcement,
            TargetId = postId,
            Reason = reason?.Trim(),
            Status = ReportStatus.Open,
            CreatedAt = DateTime.UtcNow,
        };

        await this.reportRepository.AddAsync(report);
        await this.reportRepository.SaveChangesAsync();

        this.logger.LogInformation("User {ReporterId} reported post {PostId}", reporterId, postId);
        return report;
    }

    public async Task<bool> DeleteAnnouncementByReportAsync(int reportId)
    {
        var report = await this.reportRepository.GetByIdAsync(reportId);
        if (report is null)
        {
            this.logger.LogWarning("DeleteAnnouncementByReportAsync: report {ReportId} not found", reportId);
            return false;
        }

        if (report.TargetType != TargetType.Announcement)
        {
            this.logger.LogWarning("DeleteAnnouncementByReportAsync: report {ReportId} is not for an Announcement", reportId);
            return false;
        }

        var announcementId = report.TargetId;

        var announcement = await this.announcementRepository.GetByIdAsync(announcementId);
        if (announcement is null)
        {
            this.logger.LogWarning("DeleteAnnouncementByReportAsync: announcement {AnnouncementId} not found", announcementId);
            this.reportRepository.Delete(report);
            await this.reportRepository.SaveChangesAsync();
            return false;
        }

        var relatedReports = await this.reportRepository.GetByTargetAsync(TargetType.Announcement, announcementId);
        foreach (var r in relatedReports)
        {
            this.reportRepository.Delete(r);
        }

        this.announcementRepository.Delete(announcement);

        await this.announcementRepository.SaveChangesAsync();
        await this.reportRepository.SaveChangesAsync();

        this.logger.LogInformation(
            "DeleteAnnouncementByReportAsync: announcement {AnnouncementId} deleted by report {ReportId}",
            announcementId,
            reportId);

        return true;
    }
}
