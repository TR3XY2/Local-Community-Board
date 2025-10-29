// <copyright file="ReactionServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Tests;

using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using LocalCommunityBoard.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="ReactionService"/>.
/// </summary>
public class ReactionServiceTests
{
    private readonly Mock<IReactionRepository> mockReactionRepository;
    private readonly Mock<ILogger<ReactionService>> mockLogger;
    private readonly ReactionService reactionService;

    public ReactionServiceTests()
    {
        mockReactionRepository = new Mock<IReactionRepository>();
        mockLogger = new Mock<ILogger<ReactionService>>();
        reactionService = new ReactionService(mockReactionRepository.Object, mockLogger.Object);
    }

    [Fact]
    public async Task ToggleLikeAsync_AddsLike_WhenUserHasNotLiked()
    {
        // Arrange
        const int announcementId = 1;
        const int userId = 1;
        Reaction? capturedReaction = null;

        mockReactionRepository
            .Setup(repo => repo.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like))
            .ReturnsAsync((Reaction?)null);

        mockReactionRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Reaction>()))
            .Callback<Reaction>(r => capturedReaction = r)
            .Returns(Task.CompletedTask);

        mockReactionRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await reactionService.ToggleLikeAsync(announcementId, userId);

        // Assert
        Assert.True(result);
        Assert.NotNull(capturedReaction);
        Assert.Equal(announcementId, capturedReaction.AnnouncementId);
        Assert.Equal(userId, capturedReaction.UserId);
        Assert.Equal(ReactionType.Like, capturedReaction.Type);
        mockReactionRepository.Verify(repo => repo.AddAsync(It.IsAny<Reaction>()), Times.Once);
        mockReactionRepository.Verify(repo => repo.Delete(It.IsAny<Reaction>()), Times.Never);
        mockReactionRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ToggleLikeAsync_RemovesLike_WhenUserHasAlreadyLiked()
    {
        // Arrange
        const int announcementId = 1;
        const int userId = 1;
        Reaction existingReaction = new Reaction
        {
            Id = 1,
            AnnouncementId = announcementId,
            UserId = userId,
            Type = ReactionType.Like,
        };

        mockReactionRepository
            .Setup(repo => repo.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like))
            .ReturnsAsync(existingReaction);

        mockReactionRepository
            .Setup(repo => repo.Delete(existingReaction))
            .Verifiable();

        mockReactionRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await reactionService.ToggleLikeAsync(announcementId, userId);

        // Assert
        Assert.False(result);
        mockReactionRepository.Verify(repo => repo.Delete(existingReaction), Times.Once);
        mockReactionRepository.Verify(repo => repo.AddAsync(It.IsAny<Reaction>()), Times.Never);
        mockReactionRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetLikesCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        const int announcementId = 1;
        const int expectedCount = 5;

        mockReactionRepository
            .Setup(repo => repo.CountByAnnouncementAsync(announcementId, ReactionType.Like))
            .ReturnsAsync(expectedCount);

        // Act
        int result = await reactionService.GetLikesCountAsync(announcementId);

        // Assert
        Assert.Equal(expectedCount, result);
        mockReactionRepository.Verify(
            repo => repo.CountByAnnouncementAsync(announcementId, ReactionType.Like),
            Times.Once);
    }

    [Fact]
    public async Task GetLikesCountAsync_ReturnsZero_WhenNoLikes()
    {
        // Arrange
        const int announcementId = 1;

        mockReactionRepository
            .Setup(repo => repo.CountByAnnouncementAsync(announcementId, ReactionType.Like))
            .ReturnsAsync(0);

        // Act
        int result = await reactionService.GetLikesCountAsync(announcementId);

        // Assert
        Assert.Equal(0, result);
        mockReactionRepository.Verify(
            repo => repo.CountByAnnouncementAsync(announcementId, ReactionType.Like),
            Times.Once);
    }

    [Fact]
    public async Task HasUserLikedAsync_ReturnsTrue_WhenUserHasLiked()
    {
        // Arrange
        const int announcementId = 1;
        const int userId = 1;
        Reaction existingReaction = new Reaction
        {
            Id = 1,
            AnnouncementId = announcementId,
            UserId = userId,
            Type = ReactionType.Like,
        };

        mockReactionRepository
            .Setup(repo => repo.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like))
            .ReturnsAsync(existingReaction);

        // Act
        bool result = await reactionService.HasUserLikedAsync(announcementId, userId);

        // Assert
        Assert.True(result);
        mockReactionRepository.Verify(
            repo => repo.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like),
            Times.Once);
    }

    [Fact]
    public async Task HasUserLikedAsync_ReturnsFalse_WhenUserHasNotLiked()
    {
        // Arrange
        const int announcementId = 1;
        const int userId = 1;

        mockReactionRepository
            .Setup(repo => repo.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like))
            .ReturnsAsync((Reaction?)null);

        // Act
        bool result = await reactionService.HasUserLikedAsync(announcementId, userId);

        // Assert
        Assert.False(result);
        mockReactionRepository.Verify(
            repo => repo.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like),
            Times.Once);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 2)]
    [InlineData(10, 3)]
    public async Task ToggleLikeAsync_WorksWithDifferentIds(int announcementId, int userId)
    {
        // Arrange
        mockReactionRepository
            .Setup(repo => repo.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like))
            .ReturnsAsync((Reaction?)null);

        mockReactionRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Reaction>()))
            .Returns(Task.CompletedTask);

        mockReactionRepository
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await reactionService.ToggleLikeAsync(announcementId, userId);

        // Assert
        Assert.True(result);
        mockReactionRepository.Verify(
            repo => repo.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like),
            Times.Once);
    }
}
