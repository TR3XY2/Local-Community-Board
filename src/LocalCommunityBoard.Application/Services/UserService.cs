// <copyright file="UserService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Services;

using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Security;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using LocalCommunityBoard.Domain.Interfaces;

/// <summary>
/// Provides business logic for user management (registration, login, etc.).
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="userRepository">Repository for user data access.</param>
    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    /// <summary>
    /// Registers a new user with a securely hashed password.
    /// </summary>
    public async Task<User> RegisterAsync(string username, string email, string password)
    {
        if (await this.userRepository.EmailExistsAsync(email))
        {
            throw new InvalidOperationException("Email already in use");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            Password = PasswordHasher.HashPassword(password),
            RoleId = 1,
            Status = UserStatus.Active,
        };

        await this.userRepository.AddAsync(user);
        await this.userRepository.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Authenticates a user by verifying email and password.
    /// </summary>
    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await this.userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        var isValid = PasswordHasher.VerifyPassword(password, user.Password);
        return isValid ? user : null;
    }

    public async Task<User?> GetByIdAsync(int id) => await this.userRepository.GetByIdAsync(id);

    public async Task<bool> UpdateAsync(User user)
    {
        this.userRepository.Update(user);
        await this.userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await this.userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        this.userRepository.Delete(user);
        await this.userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await this.userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!PasswordHasher.VerifyPassword(oldPassword, user.Password))
        {
            throw new UnauthorizedAccessException("Old password is incorrect");
        }

        user.Password = PasswordHasher.HashPassword(newPassword);
        this.userRepository.Update(user);
        await this.userRepository.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Updates the personal information of a user (e.g., username, email).
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    /// <param name="newUsername">The new username, or null to keep the current one.</param>
    /// <param name="newEmail">The new email, or null to keep the current one.</param>
    /// <returns>True if the update was successful; false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the user does not exist or email is already in use.</exception>
    public async Task<bool> UpdatePersonalInfoAsync(int userId, string? newUsername, string? newEmail)
    {
        var user = await this.userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if email is already in use by another user
        if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != user.Email)
        {
            if (await this.userRepository.EmailExistsAsync(newEmail))
            {
                throw new InvalidOperationException("Email is already in use by another account.");
            }

            user.Email = newEmail;
        }

        // Update username if provided
        if (!string.IsNullOrWhiteSpace(newUsername))
        {
            user.Username = newUsername;
        }

        this.userRepository.Update(user);
        await this.userRepository.SaveChangesAsync();
        return true;
    }
}
