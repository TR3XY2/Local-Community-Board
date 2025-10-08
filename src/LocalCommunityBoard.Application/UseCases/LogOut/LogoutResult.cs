// © PlaceholderCompany
// <copyright file="LogoutResult.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.UseCases.Logout;

/// <summary>
/// Result of logout operation.
/// </summary>
public sealed class LogoutResult
{
    /// <summary>
    /// Gets a value indicating whether the logout finished successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets an informational message about the outcome.
    /// </summary>
    public string? Message { get; init; }
}
