// <copyright file="ICommentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Interfaces;

using LocalCommunityBoard.Domain.Entities;

public interface ICommentService
{
    Task<IEnumerable<Comment>> GetCommentsForAnnouncementAsync(int announcementId);

    Task<Comment> AddCommentAsync(int userId, int announcementId, string body, int? parentCommentId = null);
}
