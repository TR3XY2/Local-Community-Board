// <copyright file="IReactionRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Interfaces;

using System.Threading.Tasks;

using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;

/// <summary>
/// Defines repository operations specific to reactions (likes).
/// </summary>
public interface IReactionRepository : IRepository<Reaction>
{
    /// <summary>
    /// Gets a reaction for a given announcement and user (if any).
    /// </summary>
    /// <param name="announcementId">The unique identifier of the announcement.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="type">The reaction type to retrieve (defaults to like).</param>
    /// <returns>The matching <see cref="Reaction"/> if it exists; otherwise, null.</returns>
    Task<Reaction?> GetByAnnouncementAndUserAsync(int announcementId, int userId, ReactionType type = ReactionType.Like);

    /// <summary>
    /// Counts reactions of a given type for an announcement.
    /// </summary>
    /// <param name="announcementId">The unique identifier of the announcement.</param>
    /// <param name="type">The reaction type to count (defaults to like).</param>
    /// <returns>The number of reactions of the specified type for the announcement.</returns>
    Task<int> CountByAnnouncementAsync(int announcementId, ReactionType type = ReactionType.Like);
}
