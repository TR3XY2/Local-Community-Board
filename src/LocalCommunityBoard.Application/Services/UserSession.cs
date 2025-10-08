// <copyright file="UserSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Services;

using LocalCommunityBoard.Domain.Entities;

/// <summary>
/// Maintains the current user's session state in the WPF app.
/// </summary>
public class UserSession
{
    /// <summary>
    /// Gets the currently logged-in user, or null if no user is logged in.
    /// </summary>
    public User? CurrentUser { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a user is currently logged in.
    /// </summary>
    public bool IsLoggedIn => this.CurrentUser != null;

    /// <summary>
    /// Gets the username of the current user, or "Guest" if none.
    /// </summary>
    public string DisplayName => this.CurrentUser?.Username ?? "Guest";

    /// <summary>
    /// Logs in the specified user.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    public void Login(User user)
    {
        this.CurrentUser = user;
    }

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    public void Logout()
    {
        this.CurrentUser = null;
    }
}
