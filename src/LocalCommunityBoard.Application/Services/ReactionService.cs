// <copyright file="ReactionService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Services;

using System.Threading.Tasks;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using LocalCommunityBoard.Domain.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides reaction (like) related operations.
/// </summary>
public class ReactionService : IReactionService
{
    private readonly IReactionRepository reactionRepository;
    private readonly ILogger<ReactionService> logger;

    public ReactionService(IReactionRepository reactionRepository, ILogger<ReactionService> logger)
    {
        this.reactionRepository = reactionRepository;
        this.logger = logger;
    }

    /// <summary>
    /// Toggles the "like" reaction for a specific announcement by a user.
    /// </summary>
    /// <remarks>If the user has already liked the announcement, this method removes the like. Otherwise, it
    /// adds a like for the announcement. The operation is logged at each step.</remarks>
    /// <param name="announcementId">The unique identifier of the announcement to toggle the like for.</param>
    /// <param name="userId">The unique identifier of the user performing the action.</param>
    /// <returns><see langword="true"/> if the announcement is now liked by the user; otherwise, <see langword="false"/> if the
    /// like was removed.</returns>
    public async Task<bool> ToggleLikeAsync(int announcementId, int userId)
    {
        this.logger.LogInformation("User {UserId} toggled like for announcement {AnnouncementId}", userId, announcementId);

        var existing = await this.reactionRepository.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like);
        if (existing != null)
        {
            this.reactionRepository.Delete(existing);
            await this.reactionRepository.SaveChangesAsync();

            this.logger.LogInformation("User {UserId} removed like from announcement {AnnouncementId}", userId, announcementId);
            return false; // now unliked
        }

        var reaction = new Reaction
        {
            AnnouncementId = announcementId,
            UserId = userId,
            Type = ReactionType.Like,
        };
        await this.reactionRepository.AddAsync(reaction);
        await this.reactionRepository.SaveChangesAsync();

        this.logger.LogInformation("User {UserId} liked announcement {AnnouncementId}", userId, announcementId);
        return true; // now liked
    }

    /// <inheritdoc />
    public Task<int> GetLikesCountAsync(int announcementId) => this.reactionRepository.CountByAnnouncementAsync(announcementId, ReactionType.Like);

    /// <inheritdoc />
    public async Task<bool> HasUserLikedAsync(int announcementId, int userId) => (await this.reactionRepository.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like)) != null;
}
