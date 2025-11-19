// <copyright file="IUserService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Interfaces;

using LocalCommunityBoard.Domain.Entities;

public interface IUserService
{
    Task<User> RegisterAsync(string username, string email, string password);

    Task<User?> LoginAsync(string email, string password);

    Task<User?> GetByIdAsync(int id);

    Task<bool> UpdateAsync(User user);

    Task<bool> DeleteAsync(int id);

    Task<bool> UpdatePersonalInfoAsync(int userId, string? newUsername, string? newEmail);

    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

    Task<IEnumerable<User>> GetAllUsersAsync();

    Task<bool> AdminUpdateUserAsync(int userId, string? newUsername, string? newEmail, string? newPassword);

    Task<bool> UnblockUserAsync(int userId);

    Task<bool> BlockUserAsync(int userId);

    Task<bool> DeleteUserByAdminAsync(int userId);

    Task<bool> DeleteOwnAccountAsync(int userId, string password);
}
