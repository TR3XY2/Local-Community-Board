// <copyright file="Repository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Infrastructure.Repositories;

using LocalCommunityBoard.Domain.Interfaces;
using LocalCommunityBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Generic repository implementation for managing entities.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="Repository{T}"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
public class Repository<T>(LocalCommunityBoardDbContext context) : IRepository<T>
    where T : class
{
    /// <summary>
    /// Gets the database context.
    /// </summary>
    protected LocalCommunityBoardDbContext Context { get; } = context;

    /// <summary>
    /// Gets the database set for the entity type.
    /// </summary>
    protected DbSet<T> DbSet { get; } = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id) => await this.DbSet.FindAsync(id);

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await this.DbSet.ToListAsync();

    public async Task AddAsync(T entity) => await this.DbSet.AddAsync(entity);

    public void Update(T entity) => this.DbSet.Update(entity);

    public void Delete(T entity) => this.DbSet.Remove(entity);

    public async Task SaveChangesAsync() => await this.Context.SaveChangesAsync();
}
