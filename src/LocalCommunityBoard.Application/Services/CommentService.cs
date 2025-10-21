// <copyright file="CommentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Services;

using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Interfaces;

public class CommentService : ICommentService
{
    private readonly ICommentRepository commentRepository;

    public CommentService(ICommentRepository commentRepository)
    {
        this.commentRepository = commentRepository;
    }

    public async Task<IEnumerable<Comment>> GetCommentsForAnnouncementAsync(int announcementId)
    {
        return await this.commentRepository.GetByAnnouncementIdAsync(announcementId);
    }

    public async Task<Comment> AddCommentAsync(int userId, int announcementId, string body, int? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Comment cannot be empty.");
        }

        var comment = new Comment
        {
            UserId = userId,
            AnnouncementId = announcementId,
            Body = body.Trim(),
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.UtcNow,
        };

        await this.commentRepository.AddAsync(comment);
        await this.commentRepository.SaveChangesAsync();

        return comment;
    }
}
