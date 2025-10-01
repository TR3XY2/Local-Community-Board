// <copyright file="LocalCommunityBoardDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Infrastructure.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using LocalCommunityBoard.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Represents the database context for the Local Community Board application.
    /// </summary>
    public class LocalCommunityBoardDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalCommunityBoardDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to configure the database context.</param>
        public LocalCommunityBoardDbContext(DbContextOptions<LocalCommunityBoardDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets the Users table.
        /// </summary>
        public DbSet<User> Users => this.Set<User>();

        /// <summary>
        /// Gets the Roles table.
        /// </summary>
        public DbSet<Role> Roles => this.Set<Role>();

        /// <summary>
        /// Gets the Categories table.
        /// </summary>
        public DbSet<Category> Categories => this.Set<Category>();

        /// <summary>
        /// Gets the Locations table.
        /// </summary>
        public DbSet<Location> Locations => this.Set<Location>();

        /// <summary>
        /// Gets the Announcements table.
        /// </summary>
        public DbSet<Announcement> Announcements => this.Set<Announcement>();

        /// <summary>
        /// Gets the Comments table.
        /// </summary>
        public DbSet<Comment> Comments => this.Set<Comment>();

        /// <summary>
        /// Gets the Reactions table.
        /// </summary>
        public DbSet<Reaction> Reactions => this.Set<Reaction>();

        /// <summary>
        /// Gets the Subscriptions table.
        /// </summary>
        public DbSet<Subscription> Subscriptions => this.Set<Subscription>();

        /// <summary>
        /// Gets the Reports table.
        /// </summary>
        public DbSet<Report> Reports => this.Set<Report>();

        /// <summary>
        /// Gets the ModerationActions table.
        /// </summary>
        public DbSet<ModerationAction> ModerationActions => this.Set<ModerationAction>();

        /// <summary>
        /// Configures the model for the database context.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure the database schema.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Announcement>()
                .Property(a => a.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Reaction>()
                .Property(r => r.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Report>()
                .Property(r => r.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Report>()
                .Property(r => r.TargetType)
                .HasConversion<string>();

            modelBuilder.Entity<ModerationAction>()
                .Property(m => m.Action)
                .HasConversion<string>();

            modelBuilder.Entity<ModerationAction>()
                .Property(m => m.TargetType)
                .HasConversion<string>();

            // Unique index on Reaction
            modelBuilder.Entity<Reaction>()
                .HasIndex(r => new { r.AnnouncementId, r.UserId })
                .IsUnique();

            // Self-reference for comments
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany()
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
