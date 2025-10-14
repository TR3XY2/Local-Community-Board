// <copyright file="ReactionService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Services;

using System.Threading.Tasks;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using LocalCommunityBoard.Domain.Interfaces;

/// <summary>
/// Provides reaction (like) related operations.
/// </summary>
public class ReactionService(IReactionRepository reactionRepository) : IReactionService
{
    private readonly IReactionRepository reactionRepository = reactionRepository;

    /// <inheritdoc />
    public async Task<bool> ToggleLikeAsync(int announcementId, int userId)
    {
        var existing = await this.reactionRepository.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like);
        if (existing != null)
        {
            this.reactionRepository.Delete(existing);
            await this.reactionRepository.SaveChangesAsync();
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
        return true; // now liked
    }

    /// <inheritdoc />
    public Task<int> GetLikesCountAsync(int announcementId) => this.reactionRepository.CountByAnnouncementAsync(announcementId, ReactionType.Like);

    /// <inheritdoc />
    public async Task<bool> HasUserLikedAsync(int announcementId, int userId) => (await this.reactionRepository.GetByAnnouncementAndUserAsync(announcementId, userId, ReactionType.Like)) != null;
}
