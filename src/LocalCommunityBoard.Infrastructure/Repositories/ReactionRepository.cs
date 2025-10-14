// <copyright file="ReactionRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Infrastructure.Repositories;

using System.Threading.Tasks;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using LocalCommunityBoard.Domain.Interfaces;
using LocalCommunityBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repository for managing reactions (likes/dislikes).
/// </summary>
public class ReactionRepository : Repository<Reaction>, IReactionRepository
{
    public ReactionRepository(LocalCommunityBoardDbContext context)
        : base(context)
    {
    }

    public async Task<Reaction?> GetByAnnouncementAndUserAsync(int announcementId, int userId, ReactionType type = ReactionType.Like)
    {
        return await this.Context.Reactions.FirstOrDefaultAsync(r => r.AnnouncementId == announcementId && r.UserId == userId && r.Type == type);
    }

    public async Task<int> CountByAnnouncementAsync(int announcementId, ReactionType type = ReactionType.Like)
    {
        return await this.Context.Reactions.CountAsync(r => r.AnnouncementId == announcementId && r.Type == type);
    }
}
