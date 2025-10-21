// <copyright file="CommentRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Infrastructure.Repositories;

using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Interfaces;
using LocalCommunityBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(LocalCommunityBoardDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<Comment>> GetByAnnouncementIdAsync(int announcementId)
    {
        return await this.DbSet
            .Include(c => c.User)
            .Where(c => c.AnnouncementId == announcementId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}
