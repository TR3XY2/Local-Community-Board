// <copyright file="LocalCommunityBoardDbContextFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Infrastructure.Data;

using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

/// <summary>
/// A factory for creating instances of <see cref="LocalCommunityBoardDbContext"/> at design time.
/// </summary>
public class LocalCommunityBoardDbContextFactory
    : IDesignTimeDbContextFactory<LocalCommunityBoardDbContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="LocalCommunityBoardDbContext"/> using the specified arguments.
    /// </summary>
    /// <param name="args">The arguments for creating the DbContext.</param>
    /// <returns>A new instance of <see cref="LocalCommunityBoardDbContext"/>.</returns>
    public LocalCommunityBoardDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<LocalCommunityBoardDbContextFactory>()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<LocalCommunityBoardDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Postgres"));

        return new LocalCommunityBoardDbContext(optionsBuilder.Options);
    }
}
