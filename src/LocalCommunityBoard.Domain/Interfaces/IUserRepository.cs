// <copyright file="IUserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Interfaces;

using LocalCommunityBoard.Domain.Entities;

/// <summary>
/// Interface for user-related data access operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByUsernameAsync(string username);

    Task<User?> GetByPhoneNumberAsync(string phoneNumber);

    Task<bool> EmailExistsAsync(string email);

    Task<bool> UsernameExistsAsync(string username);

    Task<bool> PhoneNumberExistsAsync(string phoneNumber);
}
