// <copyright file="CommentServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Tests.Application.Services;

using System.Collections.Generic;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="CommentService"/>.
/// </summary>
public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> mockCommentRepository;
    private readonly Mock<ILogger<CommentService>> mockLogger; // Add mock logger
    private readonly CommentService commentService;

    public CommentServiceTests()
    {
        this.mockCommentRepository = new Mock<ICommentRepository>();
        this.mockLogger = new Mock<ILogger<CommentService>>();
        this.commentService = new CommentService(this.mockCommentRepository.Object, this.mockLogger.Object);
    }

    [Fact]
    public async Task GetCommentsForAnnouncementAsync_ReturnsComments_WhenCommentsExist()
    {
        // Arrange
        const int announcementId = 1;
        List<Comment> expectedComments = new List<Comment>
        {
            new Comment { Id = 1, AnnouncementId = announcementId, Body = "Test comment 1", UserId = 1 },
            new Comment { Id = 2, AnnouncementId = announcementId, Body = "Test comment 2", UserId = 2 },
        };

        this.mockCommentRepository
            .Setup(repo => repo.GetByAnnouncementIdAsync(announcementId))
            .ReturnsAsync(expectedComments);

        // Act
        IEnumerable<Comment> result = await this.commentService.GetCommentsForAnnouncementAsync(announcementId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        this.mockCommentRepository.Verify(repo => repo.GetByAnnouncementIdAsync(announcementId), Times.Once);
    }

    [Fact]
    public async Task GetCommentsForAnnouncementAsync_ReturnsEmptyList_WhenNoCommentsExist()
    {
        // Arrange
        const int announcementId = 1;
        List<Comment> expectedComments = [];

        this.mockCommentRepository
            .Setup(repo => repo.GetByAnnouncementIdAsync(announcementId))
            .ReturnsAsync(expectedComments);

        // Act
        IEnumerable<Comment> result = await this.commentService.GetCommentsForAnnouncementAsync(announcementId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        this.mockCommentRepository.Verify(repo => repo.GetByAnnouncementIdAsync(announcementId), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_CreatesComment_WhenValidDataProvided()
    {
        // Arrange
        const int userId = 1;
        const int announcementId = 1;
        const string body = "This is a test comment";
        Comment? capturedComment = null;

        this.mockCommentRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Comment>()))
            .Callback<Comment>(c => capturedComment = c)
            .Returns(Task.CompletedTask);

        this.mockCommentRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        Comment result = await this.commentService.AddCommentAsync(userId, announcementId, body);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(announcementId, result.AnnouncementId);
        Assert.Equal(body, result.Body);
        Assert.Null(result.ParentCommentId);
        Assert.NotNull(capturedComment);
        this.mockCommentRepository.Verify(repo => repo.AddAsync(It.IsAny<Comment>()), Times.Once);
        this.mockCommentRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_CreatesReply_WhenParentCommentIdProvided()
    {
        // Arrange
        const int userId = 1;
        const int announcementId = 1;
        const string body = "This is a reply";
        const int parentCommentId = 5;

        this.mockCommentRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Comment>()))
            .Returns(Task.CompletedTask);

        this.mockCommentRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        Comment result = await this.commentService.AddCommentAsync(userId, announcementId, body, parentCommentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(parentCommentId, result.ParentCommentId);
        this.mockCommentRepository.Verify(repo => repo.AddAsync(It.IsAny<Comment>()), Times.Once);
        this.mockCommentRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_TrimsWhitespace_WhenBodyHasWhitespace()
    {
        // Arrange
        const int userId = 1;
        const int announcementId = 1;
        const string body = "  Test comment with whitespace  ";
        Comment? capturedComment = null;

        this.mockCommentRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Comment>()))
            .Callback<Comment>(c => capturedComment = c)
            .Returns(Task.CompletedTask);

        this.mockCommentRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        Comment result = await this.commentService.AddCommentAsync(userId, announcementId, body);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test comment with whitespace", result.Body);
        Assert.Equal("Test comment with whitespace", capturedComment?.Body);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task AddCommentAsync_ThrowsArgumentException_WhenBodyIsNullOrWhitespace(string? invalidBody)
    {
        // Arrange
        const int userId = 1;
        const int announcementId = 1;

        #pragma warning disable CS8604
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => this.commentService.AddCommentAsync(userId, announcementId, invalidBody));
        #pragma warning restore CS8604
        Assert.Equal("Comment cannot be empty.", exception.Message);
        this.mockCommentRepository.Verify(repo => repo.AddAsync(It.IsAny<Comment>()), Times.Never);
        this.mockCommentRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AddCommentAsync_SetsCreatedAtToUtcNow()
    {
        // Arrange
        const int userId = 1;
        const int announcementId = 1;
        const string body = "Test comment";
        DateTime beforeCall = DateTime.UtcNow;
        Comment? capturedComment = null;

        this.mockCommentRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Comment>()))
            .Callback<Comment>(c => capturedComment = c)
            .Returns(Task.CompletedTask);

        this.mockCommentRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        Comment result = await this.commentService.AddCommentAsync(userId, announcementId, body);
        Console.WriteLine(result);
        DateTime afterCall = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedComment);
        Assert.InRange(capturedComment.CreatedAt, beforeCall, afterCall);
        Assert.Equal(DateTimeKind.Utc, capturedComment.CreatedAt.Kind);
    }

    [Fact]
    public async Task UpdateReportedCommentAsync_UpdatesComment_WhenValidDataProvided()
    {
        // Arrange
        const int commentId = 1;
        const string newBody = "Updated comment body";
        Comment comment = new Comment { Id = commentId, UserId = 1, Body = "Original body" };

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        this.mockCommentRepository
            .Setup(repo => repo.Update(It.IsAny<Comment>()))
            .Verifiable();

        this.mockCommentRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await this.commentService.UpdateReportedCommentAsync(commentId, newBody);

        // Assert
        Assert.True(result);
        Assert.Equal(newBody, comment.Body);
        this.mockCommentRepository.Verify(repo => repo.Update(comment), Times.Once);
        this.mockCommentRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateReportedCommentAsync_ReturnsFalse_WhenCommentDoesNotExist()
    {
        // Arrange
        const int commentId = 999;
        const string newBody = "Updated body";

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);

        // Act
        bool result = await this.commentService.UpdateReportedCommentAsync(commentId, newBody);

        // Assert
        Assert.False(result);
        this.mockCommentRepository.Verify(repo => repo.Update(It.IsAny<Comment>()), Times.Never);
        this.mockCommentRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task UpdateReportedCommentAsync_ThrowsArgumentException_WhenBodyIsNullOrWhitespace(string? invalidBody)
    {
        // Arrange
        const int commentId = 1;

        // Act & Assert
        #pragma warning disable CS8604
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => this.commentService.UpdateReportedCommentAsync(commentId, invalidBody));
        #pragma warning restore CS8604

        Assert.Equal("Comment body cannot be empty.", exception.Message);
        this.mockCommentRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
        this.mockCommentRepository.Verify(repo => repo.Update(It.IsAny<Comment>()), Times.Never);
        this.mockCommentRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateReportedCommentAsync_TrimsWhitespace_WhenBodyHasWhitespace()
    {
        // Arrange
        const int commentId = 1;
        const string newBody = "  Updated comment with whitespace  ";
        Comment comment = new Comment { Id = commentId, UserId = 1, Body = "Original body" };

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        this.mockCommentRepository
            .Setup(repo => repo.Update(It.IsAny<Comment>()))
            .Verifiable();

        this.mockCommentRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await this.commentService.UpdateReportedCommentAsync(commentId, newBody);

        // Assert
        Assert.True(result);
        Assert.Equal("Updated comment with whitespace", comment.Body);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsComment_WhenCommentExists()
    {
        // Arrange
        const int commentId = 1;
        Comment expectedComment = new Comment { Id = commentId, UserId = 1, Body = "Test comment" };

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(expectedComment);

        // Act
        Comment? result = await this.commentService.GetByIdAsync(commentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedComment.Id, result.Id);
        Assert.Equal(expectedComment.Body, result.Body);
        this.mockCommentRepository.Verify(repo => repo.GetByIdAsync(commentId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCommentDoesNotExist()
    {
        // Arrange
        const int commentId = 999;

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);

        // Act
        Comment? result = await this.commentService.GetByIdAsync(commentId);

        // Assert
        Assert.Null(result);
        this.mockCommentRepository.Verify(repo => repo.GetByIdAsync(commentId), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_DeletesComment_WhenUserIsOwner()
    {
        // Arrange
        const int commentId = 1;
        const int userId = 1;
        Comment comment = new Comment { Id = commentId, UserId = userId, Body = "Test comment" };

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        this.mockCommentRepository
            .Setup(repo => repo.Delete(It.IsAny<Comment>()))
            .Verifiable();

        this.mockCommentRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await this.commentService.DeleteCommentAsync(commentId, userId);

        // Assert
        Assert.True(result);
        this.mockCommentRepository.Verify(repo => repo.Delete(comment), Times.Once);
        this.mockCommentRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_ReturnsFalse_WhenCommentDoesNotExist()
    {
        // Arrange
        const int commentId = 999;
        const int userId = 1;

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);

        // Act
        bool result = await this.commentService.DeleteCommentAsync(commentId, userId);

        // Assert
        Assert.False(result);
        this.mockCommentRepository.Verify(repo => repo.Delete(It.IsAny<Comment>()), Times.Never);
        this.mockCommentRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCommentAsync_ThrowsUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        // Arrange
        const int commentId = 1;
        const int commentOwnerId = 1;
        const int attemptingUserId = 2;
        Comment comment = new Comment { Id = commentId, UserId = commentOwnerId, Body = "Test comment" };

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        // Act & Assert
        UnauthorizedAccessException exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => this.commentService.DeleteCommentAsync(commentId, attemptingUserId));

        Assert.Equal("You can delete only your own comments.", exception.Message);
        this.mockCommentRepository.Verify(repo => repo.Delete(It.IsAny<Comment>()), Times.Never);
        this.mockCommentRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 5)]
    [InlineData(10, 10)]
    public async Task DeleteCommentAsync_WorksWithDifferentUserIds(int commentId, int userId)
    {
        // Arrange
        Comment comment = new Comment { Id = commentId, UserId = userId, Body = "Test comment" };

        this.mockCommentRepository
            .Setup(repo => repo.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        this.mockCommentRepository
            .Setup(repo => repo.Delete(It.IsAny<Comment>()))
            .Verifiable();

        this.mockCommentRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await this.commentService.DeleteCommentAsync(commentId, userId);

        // Assert
        Assert.True(result);
        this.mockCommentRepository.Verify(repo => repo.GetByIdAsync(commentId), Times.Once);
    }
}
