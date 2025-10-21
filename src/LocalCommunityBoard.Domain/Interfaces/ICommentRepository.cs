// <copyright file="ICommentRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Interfaces;

using LocalCommunityBoard.Domain.Entities;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByAnnouncementIdAsync(int announcementId);
}
