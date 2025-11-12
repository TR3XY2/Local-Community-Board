// <copyright file="ReportServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Tests.Application.Services;

using System.Collections.Generic;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using LocalCommunityBoard.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="ReportService"/>.
/// </summary>
public class ReportServiceTests
{
    private readonly Mock<IReportRepository> mockReportRepository;
    private readonly Mock<ICommentRepository> mockCommentRepository;
    private readonly Mock<IAnnouncementRepository> mockAnnouncementRepository;
    private readonly Mock<ILogger<ReportService>> mockLogger;
    private readonly ReportService reportService;

    public ReportServiceTests()
    {
        this.mockReportRepository = new Mock<IReportRepository>();
        this.mockCommentRepository = new Mock<ICommentRepository>();
        this.mockAnnouncementRepository = new Mock<IAnnouncementRepository>();
        this.mockLogger = new Mock<ILogger<ReportService>>();

        this.reportService = new ReportService(
            this.mockReportRepository.Object,
            this.mockCommentRepository.Object,
            this.mockLogger.Object,
            this.mockAnnouncementRepository.Object);
    }

    [Fact]
    public async Task ReportCommentAsync_CreatesReport_WhenValidDataProvided()
    {
        // Arrange
        const int reporterId = 1;
        const int commentId = 2;
        const string reason = "Inappropriate content";
        Comment comment = new Comment { Id = commentId, UserId = 3, Body = "Test comment" };
        Report? capturedReport = null;

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        this.mockReportRepository
            .Setup(repo => repo.HasUserReportedAsync(reporterId, TargetType.Comment, commentId))
            .ReturnsAsync(false);

        this.mockReportRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Report>()))
            .Callback<Report>(r => capturedReport = r)
            .Returns(Task.CompletedTask);

        this.mockReportRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        Report result = await this.reportService.ReportCommentAsync(reporterId, commentId, reason);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(reporterId, result.ReporterId);
        Assert.Equal(TargetType.Comment, result.TargetType);
        Assert.Equal(commentId, result.TargetId);
        Assert.Equal(reason, result.Reason);
        Assert.Equal(ReportStatus.Open, result.Status);
        Assert.NotNull(capturedReport);
        this.mockReportRepository.Verify(repo => repo.AddAsync(It.IsAny<Report>()), Times.Once);
        this.mockReportRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ReportCommentAsync_ThrowsArgumentException_WhenCommentDoesNotExist()
    {
        // Arrange
        const int reporterId = 1;
        const int commentId = 999;
        const string reason = "Test reason";

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);

        // Act & Assert
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => this.reportService.ReportCommentAsync(reporterId, commentId, reason));

        Assert.Contains($"Comment with ID {commentId} does not exist", exception.Message);
        this.mockReportRepository.Verify(repo => repo.AddAsync(It.IsAny<Report>()), Times.Never);
        this.mockReportRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ReportCommentAsync_ThrowsInvalidOperationException_WhenUserAlreadyReported()
    {
        // Arrange
        const int reporterId = 1;
        const int commentId = 2;
        const string reason = "Test reason";
        Comment comment = new Comment { Id = commentId, UserId = 3, Body = "Test comment" };

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        this.mockReportRepository
            .Setup(repo => repo.HasUserReportedAsync(reporterId, TargetType.Comment, commentId))
            .ReturnsAsync(true);

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.reportService.ReportCommentAsync(reporterId, commentId, reason));

        Assert.Equal("You have already reported this comment.", exception.Message);
        this.mockReportRepository.Verify(repo => repo.AddAsync(It.IsAny<Report>()), Times.Never);
        this.mockReportRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ReportCommentAsync_ThrowsInvalidOperationException_WhenReportingOwnComment()
    {
        // Arrange
        const int reporterId = 1;
        const int commentId = 2;
        const string reason = "Test reason";
        Comment comment = new Comment { Id = commentId, UserId = reporterId, Body = "My comment" };

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        this.mockReportRepository
            .Setup(repo => repo.HasUserReportedAsync(reporterId, TargetType.Comment, commentId))
            .ReturnsAsync(false);

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.reportService.ReportCommentAsync(reporterId, commentId, reason));

        Assert.Equal("You cannot report your own comment.", exception.Message);
        this.mockReportRepository.Verify(repo => repo.AddAsync(It.IsAny<Report>()), Times.Never);
        this.mockReportRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ReportCommentAsync_TrimsReason_WhenReasonHasWhitespace()
    {
        // Arrange
        const int reporterId = 1;
        const int commentId = 2;
        const string reason = "  Spam content  ";
        Comment comment = new Comment { Id = commentId, UserId = 3, Body = "Test comment" };
        Report? capturedReport = null;

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        this.mockReportRepository
            .Setup(repo => repo.HasUserReportedAsync(reporterId, TargetType.Comment, commentId))
            .ReturnsAsync(false);

        this.mockReportRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Report>()))
            .Callback<Report>(r => capturedReport = r)
            .Returns(Task.CompletedTask);

        this.mockReportRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        Report result = await this.reportService.ReportCommentAsync(reporterId, commentId, reason);

        // Assert
        Assert.Equal("Spam content", result.Reason);
        Assert.Equal("Spam content", capturedReport?.Reason);
    }

    [Fact]
    public async Task ReportCommentAsync_SetsCreatedAtToUtcNow()
    {
        // Arrange
        const int reporterId = 1;
        const int commentId = 2;
        const string reason = "Test reason";
        Comment comment = new Comment { Id = commentId, UserId = 3, Body = "Test comment" };
        DateTime beforeCall = DateTime.UtcNow;
        Report? capturedReport = null;

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        this.mockReportRepository
            .Setup(repo => repo.HasUserReportedAsync(reporterId, TargetType.Comment, commentId))
            .ReturnsAsync(false);

        this.mockReportRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Report>()))
            .Callback<Report>(r => capturedReport = r)
            .Returns(Task.CompletedTask);

        this.mockReportRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        Report result = await this.reportService.ReportCommentAsync(reporterId, commentId, reason);
        DateTime afterCall = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedReport);
        Assert.InRange(capturedReport.CreatedAt, beforeCall, afterCall);
        Assert.Equal(DateTimeKind.Utc, capturedReport.CreatedAt.Kind);
    }

    [Fact]
    public async Task GetReportsForTargetAsync_ReturnsReports_WhenReportsExist()
    {
        // Arrange
        const TargetType targetType = TargetType.Comment;
        const int targetId = 1;
        List<Report> expectedReports = new List<Report>
        {
            new Report { Id = 1, TargetType = targetType, TargetId = targetId, ReporterId = 1 },
            new Report { Id = 2, TargetType = targetType, TargetId = targetId, ReporterId = 2 },
        };

        this.mockReportRepository
            .Setup(repo => repo.GetByTargetAsync(targetType, targetId))
            .ReturnsAsync(expectedReports);

        // Act
        IEnumerable<Report> result = await this.reportService.GetReportsForTargetAsync(targetType, targetId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        this.mockReportRepository.Verify(repo => repo.GetByTargetAsync(targetType, targetId), Times.Once);
    }

    [Fact]
    public async Task GetReportsForTargetAsync_ReturnsEmptyList_WhenNoReportsExist()
    {
        // Arrange
        const TargetType targetType = TargetType.Comment;
        const int targetId = 1;

        this.mockReportRepository
            .Setup(repo => repo.GetByTargetAsync(targetType, targetId))
            .ReturnsAsync(new List<Report>());

        // Act
        IEnumerable<Report> result = await this.reportService.GetReportsForTargetAsync(targetType, targetId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReportsByStatusAsync_ReturnsReports_WhenReportsExist()
    {
        // Arrange
        const ReportStatus status = ReportStatus.Open;
        List<Report> expectedReports = new List<Report>
        {
            new Report { Id = 1, Status = status, ReporterId = 1 },
            new Report { Id = 2, Status = status, ReporterId = 2 },
        };

        this.mockReportRepository
            .Setup(repo => repo.GetByStatusAsync(status))
            .ReturnsAsync(expectedReports);

        // Act
        IEnumerable<Report> result = await this.reportService.GetReportsByStatusAsync(status);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        this.mockReportRepository.Verify(repo => repo.GetByStatusAsync(status), Times.Once);
    }

    [Theory]
    [InlineData(ReportStatus.Open)]
    [InlineData(ReportStatus.Reviewed)]
    [InlineData(ReportStatus.Closed)]
    public async Task GetReportsByStatusAsync_WorksWithAllStatuses(ReportStatus status)
    {
        // Arrange
        List<Report> expectedReports = new List<Report>
        {
            new Report { Id = 1, Status = status, ReporterId = 1 },
        };

        this.mockReportRepository
            .Setup(repo => repo.GetByStatusAsync(status))
            .ReturnsAsync(expectedReports);

        // Act
        IEnumerable<Report> result = await this.reportService.GetReportsByStatusAsync(status);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, r => Assert.Equal(status, r.Status));
    }

    [Fact]
    public async Task UpdateReportStatusAsync_UpdatesStatus_WhenReportExists()
    {
        // Arrange
        const int reportId = 1;
        const ReportStatus newStatus = ReportStatus.Reviewed;
        Report report = new Report
        {
            Id = reportId,
            Status = ReportStatus.Open,
            ReporterId = 1,
            TargetType = TargetType.Comment,
            TargetId = 1,
        };

        this.mockReportRepository
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        this.mockReportRepository
            .Setup(repo => repo.Update(It.IsAny<Report>()))
            .Verifiable();
        this.mockReportRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        // Act
        bool result = await this.reportService.UpdateReportStatusAsync(reportId, newStatus);
        // Assert
        Assert.True(result);
        Assert.Equal(newStatus, report.Status);
        this.mockReportRepository.Verify(repo => repo.Update(It.IsAny<Report>()), Times.Once
        );
        this.mockReportRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
    [Fact]
    public async Task UpdateReportStatusAsync_ReturnsFalse_WhenReportDoesNotExist()
    {
        // Arrange
        const int reportId = 999;
        const ReportStatus newStatus = ReportStatus.Reviewed;

        this.mockReportRepository
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync((Report?)null);

        // Act
        bool result = await this.reportService.UpdateReportStatusAsync(reportId, newStatus);

        // Assert
        Assert.False(result);
        this.mockReportRepository.Verify(repo => repo.Update(It.IsAny<Report>()), Times.Never);
        this.mockReportRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

[Fact]
    public async Task ReportAnnouncementAsync_CreatesReport_WhenValidDataProvided()
    {
        // Arrange
        const int reporterId = 1;
        const int announcementId = 2;
        const string reason = "Inappropriate content";
        Announcement announcement = new Announcement { Id = announcementId, Title = "Test", Body = "Test body" };
        Report? capturedReport = null;

        this.mockAnnouncementRepository
            .Setup(repo => repo.GetByIdAsync(announcementId))
            .ReturnsAsync(announcement);

        this.mockReportRepository
            .Setup(repo => repo.HasUserReportedAsync(reporterId, TargetType.Announcement, announcementId))
            .ReturnsAsync(false);

        this.mockReportRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Report>()))
            .Callback<Report>(r => capturedReport = r)
            .Returns(Task.CompletedTask);

        this.mockReportRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        Report result = await this.reportService.ReportAnnouncementAsync(reporterId, announcementId, reason);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(reporterId, result.ReporterId);
        Assert.Equal(TargetType.Announcement, result.TargetType);
        Assert.Equal(announcementId, result.TargetId);
        Assert.Equal(reason, result.Reason);
        Assert.Equal(ReportStatus.Open, result.Status);
        Assert.NotNull(capturedReport);
        this.mockReportRepository.Verify(repo => repo.AddAsync(It.IsAny<Report>()), Times.Once);
        this.mockReportRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ReportAnnouncementAsync_ThrowsArgumentException_WhenAnnouncementDoesNotExist()
    {
        // Arrange
        const int reporterId = 1;
        const int announcementId = 999;
        const string reason = "Test reason";

        this.mockAnnouncementRepository
            .Setup(repo => repo.GetByIdAsync(announcementId))
            .ReturnsAsync((Announcement?)null);

        // Act & Assert
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => this.reportService.ReportAnnouncementAsync(reporterId, announcementId, reason));

        Assert.Contains($"Post with ID {announcementId} does not exist", exception.Message);
        this.mockReportRepository.Verify(repo => repo.AddAsync(It.IsAny<Report>()), Times.Never);
        this.mockReportRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ReportAnnouncementAsync_ThrowsInvalidOperationException_WhenUserAlreadyReported()
    {
        // Arrange
        const int reporterId = 1;
        const int announcementId = 2;
        const string reason = "Test reason";
        Announcement announcement = new Announcement { Id = announcementId, Title = "Test", Body = "Test body" };

        this.mockAnnouncementRepository
            .Setup(repo => repo.GetByIdAsync(announcementId))
            .ReturnsAsync(announcement);

        this.mockReportRepository
            .Setup(repo => repo.HasUserReportedAsync(reporterId, TargetType.Announcement, announcementId))
            .ReturnsAsync(true);

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.reportService.ReportAnnouncementAsync(reporterId, announcementId, reason));

        Assert.Equal("You have already reported this post.", exception.Message);
        this.mockReportRepository.Verify(repo => repo.AddAsync(It.IsAny<Report>()), Times.Never);
        this.mockReportRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ReportAnnouncementAsync_TrimsReason_WhenReasonHasWhitespace()
    {
        // Arrange
        const int reporterId = 1;
        const int announcementId = 2;
        const string reason = "  Spam announcement  ";
        Announcement announcement = new Announcement { Id = announcementId, Title = "Test", Body = "Test body" };
        Report? capturedReport = null;

        this.mockAnnouncementRepository
            .Setup(repo => repo.GetByIdAsync(announcementId))
            .ReturnsAsync(announcement);

        this.mockReportRepository
            .Setup(repo => repo.HasUserReportedAsync(reporterId, TargetType.Announcement, announcementId))
            .ReturnsAsync(false);

        this.mockReportRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Report>()))
            .Callback<Report>(r => capturedReport = r)
            .Returns(Task.CompletedTask);

        this.mockReportRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        Report result = await this.reportService.ReportAnnouncementAsync(reporterId, announcementId, reason);

        // Assert
        Assert.Equal("Spam announcement", result.Reason);
        Assert.Equal("Spam announcement", capturedReport?.Reason);
    }

    [Fact]
    public async Task ReportAnnouncementAsync_SetsCreatedAtToUtcNow()
    {
        // Arrange
        const int reporterId = 1;
        const int announcementId = 2;
        const string reason = "Test reason";
        Announcement announcement = new Announcement { Id = announcementId, Title = "Test", Body = "Test body" };
        DateTime beforeCall = DateTime.UtcNow;
        Report? capturedReport = null;

        this.mockAnnouncementRepository
            .Setup(repo => repo.GetByIdAsync(announcementId))
            .ReturnsAsync(announcement);

        this.mockReportRepository
            .Setup(repo => repo.HasUserReportedAsync(reporterId, TargetType.Announcement, announcementId))
            .ReturnsAsync(false);

        this.mockReportRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Report>()))
            .Callback<Report>(r => capturedReport = r)
            .Returns(Task.CompletedTask);

        this.mockReportRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        Report result = await this.reportService.ReportAnnouncementAsync(reporterId, announcementId, reason);
        DateTime afterCall = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedReport);
        Assert.InRange(capturedReport.CreatedAt, beforeCall, afterCall);
        Assert.Equal(DateTimeKind.Utc, capturedReport.CreatedAt.Kind);
    }

    [Fact]
    public async Task DeleteCommentByReportAsync_ReturnsFalse_WhenReportNotFound()
    {
        // Arrange
        const int reportId = 999;
        mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync((Report?)null);

        // Act
        bool result = await reportService.DeleteCommentByReportAsync(reportId);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"report {reportId} not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockReportRepository.Verify(r => r.Delete(It.IsAny<Report>()), Times.Never);
        mockCommentRepository.Verify(r => r.Delete(It.IsAny<Comment>()), Times.Never);
        mockReportRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCommentByReportAsync_ReturnsFalse_WhenReportIsNotForComment()
    {
        // Arrange
        const int reportId = 1;
        var report = new Report { Id = reportId, TargetType = TargetType.Announcement, TargetId = 10 };
        mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        // Act
        bool result = await reportService.DeleteCommentByReportAsync(reportId);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"report {reportId} is not for a Comment")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockReportRepository.Verify(r => r.Delete(It.IsAny<Report>()), Times.Never);
        mockCommentRepository.Verify(r => r.Delete(It.IsAny<Comment>()), Times.Never);
        mockReportRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCommentByReportAsync_DeletesReportOnly_WhenCommentNotFound()
    {
        // Arrange
        const int reportId = 1;
        const int commentId = 5;
        var report = new Report { Id = reportId, TargetType = TargetType.Comment, TargetId = commentId };
        Report? capturedReport = null;

        mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);
        mockCommentRepository
            .Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);
        mockReportRepository
            .Setup(r => r.Delete(It.IsAny<Report>()))
            .Callback<Report>(r => capturedReport = r)
            .Verifiable();
        mockReportRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await reportService.DeleteCommentByReportAsync(reportId);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"comment {commentId} not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockReportRepository.Verify(r => r.Delete(report), Times.Once);
        mockReportRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        mockCommentRepository.Verify(r => r.Delete(It.IsAny<Comment>()), Times.Never);
        mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCommentByReportAsync_DeletesCommentAndAllRelatedReports_WhenValid()
    {
        // Arrange
        const int reportId = 1;
        const int commentId = 10;
        var triggeringReport = new Report { Id = reportId, TargetType = TargetType.Comment, TargetId = commentId };
        var relatedReports = new List<Report>
        {
            new Report { Id = 2, TargetType = TargetType.Comment, TargetId = commentId },
            new Report { Id = 3, TargetType = TargetType.Comment, TargetId = commentId }
        };
        var comment = new Comment { Id = commentId, UserId = 4, Body = "offending comment" };

        mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(triggeringReport);
        mockCommentRepository
            .Setup(r => r.GetByIdAsync(commentId))
            .ReturnsAsync(comment);
        mockReportRepository
            .Setup(r => r.GetByTargetAsync(TargetType.Comment, commentId))
            .ReturnsAsync(relatedReports);
        mockReportRepository
            .Setup(r => r.Delete(It.IsAny<Report>()))
            .Verifiable();
        mockCommentRepository
            .Setup(r => r.Delete(It.IsAny<Comment>()))
            .Verifiable();
        mockReportRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        mockCommentRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await reportService.DeleteCommentByReportAsync(reportId);

        // Assert
        Assert.True(result);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!
                    .Contains($"comment {commentId} deleted by report {reportId}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // All reports for the comment (including the triggering one) must be deleted
        mockReportRepository.Verify(r => r.Delete(It.IsAny<Report>()), Times.Exactly(relatedReports.Count));
        mockCommentRepository.Verify(r => r.Delete(comment), Times.Once);
        mockReportRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        mockCommentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAnnouncementByReportAsync_ReturnsFalse_WhenReportNotFound()
    {
        // Arrange
        const int reportId = 999;
        mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync((Report?)null);

        // Act
        bool result = await reportService.DeleteAnnouncementByReportAsync(reportId);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"report {reportId} not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockReportRepository.Verify(r => r.Delete(It.IsAny<Report>()), Times.Never);
        mockAnnouncementRepository.Verify(r => r.Delete(It.IsAny<Announcement>()), Times.Never);
        mockReportRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        mockAnnouncementRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAnnouncementByReportAsync_ReturnsFalse_WhenReportIsNotForAnnouncement()
    {
        // Arrange
        const int reportId = 1;
        var report = new Report { Id = reportId, TargetType = TargetType.Comment, TargetId = 10 };
        mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        // Act
        bool result = await reportService.DeleteAnnouncementByReportAsync(reportId);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"report {reportId} is not for an Announcement")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockReportRepository.Verify(r => r.Delete(It.IsAny<Report>()), Times.Never);
        mockAnnouncementRepository.Verify(r => r.Delete(It.IsAny<Announcement>()), Times.Never);
        mockReportRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        mockAnnouncementRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAnnouncementByReportAsync_DeletesReportOnly_WhenAnnouncementNotFound()
    {
        // Arrange
        const int reportId = 1;
        const int announcementId = 5;
        var report = new Report { Id = reportId, TargetType = TargetType.Announcement, TargetId = announcementId };
        mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);
        mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(announcementId))
            .ReturnsAsync((Announcement?)null);
        mockReportRepository
            .Setup(r => r.Delete(It.IsAny<Report>()))
            .Verifiable();
        mockReportRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await reportService.DeleteAnnouncementByReportAsync(reportId);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"announcement {announcementId} not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockReportRepository.Verify(r => r.Delete(report), Times.Once);
        mockReportRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        mockAnnouncementRepository.Verify(r => r.Delete(It.IsAny<Announcement>()), Times.Never);
        mockAnnouncementRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAnnouncementByReportAsync_DeletesAnnouncementAndAllRelatedReports_WhenValid()
    {
        // Arrange
        const int reportId = 1;
        const int announcementId = 20;
        var triggeringReport = new Report { Id = reportId, TargetType = TargetType.Announcement, TargetId = announcementId };
        var relatedReports = new List<Report>
        {
            new Report { Id = 2, TargetType = TargetType.Announcement, TargetId = announcementId },
            new Report { Id = 3, TargetType = TargetType.Announcement, TargetId = announcementId }
        };
        var announcement = new Announcement { Id = announcementId, Title = "spam", Body = "spam" };

        mockReportRepository
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(triggeringReport);
        mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(announcementId))
            .ReturnsAsync(announcement);
        mockReportRepository
            .Setup(r => r.GetByTargetAsync(TargetType.Announcement, announcementId))
            .ReturnsAsync(relatedReports);
        mockReportRepository
            .Setup(r => r.Delete(It.IsAny<Report>()))
            .Verifiable();
        mockAnnouncementRepository
            .Setup(r => r.Delete(It.IsAny<Announcement>()))
            .Verifiable();
        mockReportRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        mockAnnouncementRepository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await reportService.DeleteAnnouncementByReportAsync(reportId);

        // Assert
        Assert.True(result);
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!
                    .Contains($"announcement {announcementId} deleted by report {reportId}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockReportRepository.Verify(r => r.Delete(It.IsAny<Report>()), Times.Exactly(relatedReports.Count));
        mockAnnouncementRepository.Verify(r => r.Delete(announcement), Times.Once);
        mockReportRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        mockAnnouncementRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAnnouncementByReportAsync_ReturnsAnnouncement_WhenReportAndAnnouncementExist()
    {
        // Arrange
        const int reportId = 1;
        const int announcementId = 10;
        var report = new Report
        {
            Id = reportId,
            TargetType = TargetType.Announcement,
            TargetId = announcementId,
            ReporterId = 1,
        };
        var announcement = new Announcement
        {
            Id = announcementId,
            Title = "Test Announcement",
            Body = "Test Body",
        };

        this.mockReportRepository
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        this.mockAnnouncementRepository
            .Setup(repo => repo.GetByIdAsync(announcementId))
            .ReturnsAsync(announcement);

        // Act
        Announcement? result = await this.reportService.GetAnnouncementByReportAsync(reportId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(announcementId, result.Id);
        Assert.Equal("Test Announcement", result.Title);
        Assert.Equal("Test Body", result.Body);
        this.mockReportRepository.Verify(repo => repo.GetByIdAsync(reportId), Times.Once);
        this.mockAnnouncementRepository.Verify(repo => repo.GetByIdAsync(announcementId), Times.Once);
    }

    [Fact]
    public async Task GetAnnouncementByReportAsync_ReturnsNull_WhenReportDoesNotExist()
    {
        // Arrange
        const int reportId = 999;

        this.mockReportRepository
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync((Report?)null);

        // Act
        Announcement? result = await this.reportService.GetAnnouncementByReportAsync(reportId);

        // Assert
        Assert.Null(result);
        this.mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Report {reportId} invalid or not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        this.mockReportRepository.Verify(repo => repo.GetByIdAsync(reportId), Times.Once);
        this.mockAnnouncementRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAnnouncementByReportAsync_ReturnsNull_WhenReportIsNotForAnnouncement()
    {
        // Arrange
        const int reportId = 1;
        var report = new Report
        {
            Id = reportId,
            TargetType = TargetType.Comment,
            TargetId = 5,
            ReporterId = 1,
        };

        this.mockReportRepository
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        // Act
        Announcement? result = await this.reportService.GetAnnouncementByReportAsync(reportId);

        // Assert
        Assert.Null(result);
        this.mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Report {reportId} invalid or not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        this.mockReportRepository.Verify(repo => repo.GetByIdAsync(reportId), Times.Once);
        this.mockAnnouncementRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAnnouncementByReportAsync_ReturnsNull_WhenAnnouncementDoesNotExist()
    {
        // Arrange
        const int reportId = 1;
        const int announcementId = 10;
        var report = new Report
        {
            Id = reportId,
            TargetType = TargetType.Announcement,
            TargetId = announcementId,
            ReporterId = 1,
        };

        this.mockReportRepository
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        this.mockAnnouncementRepository
            .Setup(repo => repo.GetByIdAsync(announcementId))
            .ReturnsAsync((Announcement?)null);

        // Act
        Announcement? result = await this.reportService.GetAnnouncementByReportAsync(reportId);

        // Assert
        Assert.Null(result);
        this.mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Announcement {announcementId} not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        this.mockReportRepository.Verify(repo => repo.GetByIdAsync(reportId), Times.Once);
        this.mockAnnouncementRepository.Verify(repo => repo.GetByIdAsync(announcementId), Times.Once);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(5, 20)]
    [InlineData(10, 30)]
    public async Task GetAnnouncementByReportAsync_WorksWithDifferentIds(int reportId, int announcementId)
    {
        // Arrange
        var report = new Report
        {
            Id = reportId,
            TargetType = TargetType.Announcement,
            TargetId = announcementId,
            ReporterId = 1,
        };
        var announcement = new Announcement
        {
            Id = announcementId,
            Title = "Test",
            Body = "Test",
        };

        this.mockReportRepository
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        this.mockAnnouncementRepository
            .Setup(repo => repo.GetByIdAsync(announcementId))
            .ReturnsAsync(announcement);

        // Act
        Announcement? result = await this.reportService.GetAnnouncementByReportAsync(reportId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(announcementId, result.Id);
        this.mockReportRepository.Verify(repo => repo.GetByIdAsync(reportId), Times.Once);
        this.mockAnnouncementRepository.Verify(repo => repo.GetByIdAsync(announcementId), Times.Once);
    }
}
