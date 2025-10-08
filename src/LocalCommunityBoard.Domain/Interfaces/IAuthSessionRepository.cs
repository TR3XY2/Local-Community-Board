// <copyright file="IAuthSessionRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;
using LocalCommunityBoard.Domain.Entities;

/// <summary>Repository for authentication sessions.</summary>
public interface IAuthSessionRepository
{
    Task<AuthSession?> FindActiveAsync(Guid sessionId, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
