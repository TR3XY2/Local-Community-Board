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
    /// <param name="announcementId">The announcement identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns><c>true</c> if the announcement is now liked; otherwise <c>false</c>.</returns>
    public async Task<bool> ToggleLikeAsync(int announcementId, int userId)
    {
        this.logger.LogInformation("User {UserId} toggles LIKE for announcement {AnnouncementId}", userId, announcementId);
        var result = await this.ToggleReactionInternalAsync(announcementId, userId, ReactionType.Like);
        if (result)
        {
            this.logger.LogInformation("User {UserId} liked announcement {AnnouncementId}", userId, announcementId);
        }
        else
        {
            this.logger.LogInformation("User {UserId} removed like from announcement {AnnouncementId}", userId, announcementId);
        }

        return result;
    }

    /// <summary>
    /// Toggles the "dislike" reaction for a specific announcement by a user.
    /// </summary>
    /// <param name="announcementId">The announcement identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns><c>true</c> if the announcement is now disliked; otherwise <c>false</c>.</returns>
    public async Task<bool> ToggleDislikeAsync(int announcementId, int userId)
    {
        this.logger.LogInformation("User {UserId} toggles DISLIKE for announcement {AnnouncementId}", userId, announcementId);
        var result = await this.ToggleReactionInternalAsync(announcementId, userId, ReactionType.Dislike);
        if (result)
        {
            this.logger.LogInformation("User {UserId} disliked announcement {AnnouncementId}", userId, announcementId);
        }
        else
        {
            this.logger.LogInformation("User {UserId} removed dislike from announcement {AnnouncementId}", userId, announcementId);
        }

        return result;
    }

    /// <inheritdoc />
    public Task<int> GetLikesCountAsync(int announcementId) => this.reactionRepository.CountByAnnouncementAsync(announcementId, ReactionType.Like);

    /// <inheritdoc />
    public Task<int> GetDislikesCountAsync(int announcementId) => this.reactionRepository.CountByAnnouncementAsync(announcementId, ReactionType.Dislike);

    /// <inheritdoc />
    public async Task<bool> HasUserLikedAsync(int announcementId, int userId) => (await this.reactionRepository.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like)) != null;

    /// <inheritdoc />
    public async Task<bool> HasUserDislikedAsync(int announcementId, int userId) => (await this.reactionRepository.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Dislike)) != null;

    // Private helpers (placed after public members)
    // Pseudocode / Plan:
    // 1. Check if the same reaction (type) already exists for the announcement and user:
    //    - If yes: delete it, save changes, return false (reaction removed).
    // 2. Compute the opposite reaction type.
    // 3. Check if the opposite reaction exists:
    //    - If yes: delete it and save changes, but DO NOT return; continue to add the requested reaction.
    // 4. Add the new reaction of the requested type, save changes, return true (reaction added).
    // This ensures switching directly from like -> dislike (or vice versa) will delete the opposite and add the requested one in a single operation.
    private async Task<bool> ToggleReactionInternalAsync(int announcementId, int userId, ReactionType type)
    {
        // Check if the same reaction already exists -> remove it
        var existing = await this.reactionRepository.GetByAnnouncementAndUserAsync(announcementId, userId, type);
        if (existing != null)
        {
            this.reactionRepository.Delete(existing);
            await this.reactionRepository.SaveChangesAsync();
            return false; // reaction removed
        }

        // Remove opposite reaction if present (so user can't have both)
        var opposite = type == ReactionType.Like ? ReactionType.Dislike : ReactionType.Like;
        var oppositeExisting = await this.reactionRepository.GetByAnnouncementAndUserAsync(announcementId, userId, opposite);
        if (oppositeExisting != null)
        {
            this.reactionRepository.Delete(oppositeExisting);
            await this.reactionRepository.SaveChangesAsync();

            // continue to add the requested reaction below (do not return)
        }

        // Add new reaction
        await this.reactionRepository.AddAsync(new Reaction
        {
            AnnouncementId = announcementId,
            UserId = userId,
            Type = type,
        });

        await this.reactionRepository.SaveChangesAsync();
        return true; // reaction added
    }
}
