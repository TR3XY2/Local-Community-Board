// <copyright file="AuthSessionRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Infrastructure.Repositories;

using System;
using System.Threading;
using System.Threading.Tasks;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Interfaces;
using LocalCommunityBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>EF-backed repository for sessions.</summary>
public sealed class AuthSessionRepository : IAuthSessionRepository
{
    private readonly LocalCommunityBoardDbContext context;

    public AuthSessionRepository(LocalCommunityBoardDbContext context) => this.context = context;

    public Task<AuthSession?> FindActiveAsync(Guid sessionId, CancellationToken ct = default)
        => this.context.Set<AuthSession>().FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsRevoked, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => this.context.SaveChangesAsync(ct);
}
