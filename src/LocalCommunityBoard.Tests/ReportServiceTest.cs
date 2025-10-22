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
    private readonly Mock<ILogger<ReportService>> mockLogger;
    private readonly ReportService reportService;

    public ReportServiceTests()
    {
        this.mockReportRepository = new Mock<IReportRepository>();
        this.mockCommentRepository = new Mock<ICommentRepository>();
        this.mockLogger = new Mock<ILogger<ReportService>>();

        this.reportService = new ReportService(
            this.mockReportRepository.Object,
            this.mockCommentRepository.Object,
            this.mockLogger.Object);
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
}
