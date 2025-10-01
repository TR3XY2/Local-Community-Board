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

    public class LocalCommunityBoardDbContext : DbContext
    {
        public LocalCommunityBoardDbContext(DbContextOptions<LocalCommunityBoardDbContext> options)
            : base(options)
        {
        }

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

        public DbSet<User> Users => this.Set<User>();

        public DbSet<Role> Roles => this.Set<Role>();

        public DbSet<Category> Categories => this.Set<Category>();

        public DbSet<Location> Locations => this.Set<Location>();

        public DbSet<Announcement> Announcements => this.Set<Announcement>();

        public DbSet<Comment> Comments => this.Set<Comment>();

        public DbSet<Reaction> Reactions => this.Set<Reaction>();

        public DbSet<Subscription> Subscriptions => this.Set<Subscription>();

        public DbSet<Report> Reports => this.Set<Report>();

        public DbSet<ModerationAction> ModerationActions => this.Set<ModerationAction>();
    }
}
