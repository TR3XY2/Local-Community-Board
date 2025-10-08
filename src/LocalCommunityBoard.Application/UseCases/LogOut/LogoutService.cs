// <copyright file="LogoutService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.UseCases.Logout;

using System;
using System.Threading;
using System.Threading.Tasks;
using LocalCommunityBoard.Domain.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handles user/admin logout: revokes the server-side session (if active)
/// and optionally clears client-side identity (e.g., in-memory state in WPF).
/// </summary>
/// <remarks>
/// - Operation is <b>idempotent</b>: if the session is already inactive or missing, it returns success.
/// - All outcomes are logged via <see cref="ILogger"/>.
/// </remarks>
public sealed class LogoutService
{
    private readonly IAuthSessionRepository sessions;
    private readonly ILogger<LogoutService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutService"/> class.
    /// </summary>
    /// <param name="sessions">Repository for authentication sessions.</param>
    /// <param name="logger">Logger instance.</param>
    public LogoutService(IAuthSessionRepository sessions, ILogger<LogoutService> logger)
    {
        this.sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs logout for the specified session. If <paramref name="sessionId"/> is <c>null</c>,
    /// the method succeeds idempotently and only clears the client context (if provided).
    /// </summary>
    /// <param name="sessionId">Session identifier to revoke (can be <c>null</c>).</param>
    /// <param name="clearClientContext">Callback that clears client-side identity (may be <c>null</c>).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Operation result with a success flag and message.</returns>
    public async Task<LogoutResult> LogoutAsync(
        Guid? sessionId,
        Action? clearClientContext = null,
        CancellationToken ct = default)
    {
        if (sessionId is null)
        {
            this.logger.LogWarning("Logout requested but no session present. Clearing client context only.");
            clearClientContext?.Invoke();
            return new LogoutResult { Success = true, Message = "No active session." };
        }

        try
        {
            // Try to find an active (not revoked) session.
            var session = await this.sessions.FindActiveAsync(sessionId.Value, ct).ConfigureAwait(false);
            if (session is null)
            {
                this.logger.LogInformation("Logout: session {SessionId} already inactive or not found.", sessionId);
                clearClientContext?.Invoke();
                return new LogoutResult { Success = true, Message = "Session already inactive." };
            }

            // Revoke and persist.
            session.IsRevoked = true;
            session.RevokedUtc = DateTime.UtcNow;

            _ = await this.sessions.SaveChangesAsync(ct).ConfigureAwait(false);
            this.logger.LogInformation("Logout: session {SessionId} revoked successfully.", sessionId);

            clearClientContext?.Invoke();
            return new LogoutResult { Success = true, Message = "Logged out." };
        }
        catch (OperationCanceledException)
        {
            // Respect cancellation—do not treat as failure of business logic.
            this.logger.LogWarning("Logout was canceled for session {SessionId}.", sessionId);
            return new LogoutResult { Success = false, Message = "Logout canceled." };
        }
        catch (Exception ex)
        {
            // Any unexpected persistence/runtime error.
            this.logger.LogError(ex, "Logout failed while revoking session {SessionId}.", sessionId);
            return new LogoutResult { Success = false, Message = "Logout failed." };
        }
    }
}
