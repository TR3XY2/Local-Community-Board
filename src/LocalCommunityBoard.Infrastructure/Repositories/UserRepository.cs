// <copyright file="UserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Infrastructure.Repositories;

using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using LocalCommunityBoard.Domain.Interfaces;
using LocalCommunityBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repository for managing user-related data.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to be used by the repository.</param>
    public UserRepository(LocalCommunityBoardDbContext context)
        : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await this.DbSet.Include(u => u.Role)
                       .FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByUsernameAsync(string username)
        => await this.DbSet.Include(u => u.Role)
                       .FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> EmailExistsAsync(string email)
        => await this.DbSet.AnyAsync(u => u.Email == email);

    public async Task<bool> UsernameExistsAsync(string username)
        => await this.DbSet.AnyAsync(u => u.Username == username);

    public async Task<bool> SetStatusAsync(int userId, UserStatus newStatus)
    {
        var user = await this.DbSet.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return false;
        }

        user.Status = newStatus;

        await this.Context.SaveChangesAsync();
        return true;
    }
}
