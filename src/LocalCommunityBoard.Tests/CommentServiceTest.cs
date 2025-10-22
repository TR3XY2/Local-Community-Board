// <copyright file="CommentServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Tests.Application.Services;

using System.Collections.Generic;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Interfaces;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="CommentService"/>.
/// </summary>
public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> mockCommentRepository;
    private readonly CommentService commentService;

    public CommentServiceTests()
    {
        this.mockCommentRepository = new Mock<ICommentRepository>();
        this.commentService = new CommentService(this.mockCommentRepository.Object);
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
        List<Comment> expectedComments = new List<Comment>();

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
        DateTime afterCall = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedComment);
        Assert.InRange(capturedComment.CreatedAt, beforeCall, afterCall);
        Assert.Equal(DateTimeKind.Utc, capturedComment.CreatedAt.Kind);
    }
}
