// <copyright file="IReactionRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

// Domain/Interfaces/IReactionRepository.cs
namespace LocalCommunityBoard.Domain.Interfaces;

using System.Threading.Tasks;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;

/// <summary>
/// Defines repository operations specific to reactions (likes/dislikes).
/// </summary>
public interface IReactionRepository : IRepository<Reaction>
{
    Task<Reaction?> GetByAnnouncementAndUserAsync(
        int announcementId,
        int userId,
        ReactionType type = ReactionType.Like);

    Task<int> CountByAnnouncementAsync(
        int announcementId,
        ReactionType type = ReactionType.Like);
}
