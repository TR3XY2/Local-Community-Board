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

    public async Task<bool> UpdateReportedCommentAsync(int commentId, string newBody)
    {
        if (string.IsNullOrWhiteSpace(newBody))
        {
            this.logger.LogWarning("Attempted to update comment {CommentId} with empty body", commentId);
            throw new ArgumentException("Comment body cannot be empty.");
        }

        var comment = await this.commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            this.logger.LogWarning("Attempted to update non-existent comment ID {CommentId}", commentId);
            return false;
        }

        comment.Body = newBody.Trim();
        this.commentRepository.Update(comment);
        await this.commentRepository.SaveChangesAsync();

        this.logger.LogInformation("Comment {CommentId} updated successfully", commentId);
        return true;
    }

    public async Task<Comment?> GetByIdAsync(int commentId)
    {
        return await this.commentRepository.GetByIdAsync(commentId);
    }
}
