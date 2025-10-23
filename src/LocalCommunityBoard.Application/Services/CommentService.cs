// <copyright file="CommentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Services;

using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Interfaces;
using Microsoft.Extensions.Logging;

public class CommentService : ICommentService
{
    private readonly ICommentRepository commentRepository;
    private readonly ILogger<CommentService> logger;

    public CommentService(ICommentRepository commentRepository, ILogger<CommentService> logger)
    {
        this.commentRepository = commentRepository;
        this.logger = logger;
    }

    public async Task<IEnumerable<Comment>> GetCommentsForAnnouncementAsync(int announcementId)
    {
        this.logger.LogInformation("Fetching comments for announcement ID {AnnouncementId}", announcementId);
        return await this.commentRepository.GetByAnnouncementIdAsync(announcementId);
    }

    public async Task<Comment> AddCommentAsync(int userId, int announcementId, string body, int? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            this.logger.LogWarning("User {UserId} attempted to add an empty comment to announcement {AnnouncementId}", userId, announcementId);
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

        this.logger.LogInformation("User {UserId} added a comment (ID: {CommentId}) to announcement {AnnouncementId}", userId, comment.Id, announcementId);
        return comment;
    }
}
