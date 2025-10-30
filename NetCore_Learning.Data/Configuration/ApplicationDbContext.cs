using Microsoft.EntityFrameworkCore;
using NetCore_Learning.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Configuration
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<PostTag> PostTags => Set<PostTag>();
        public DbSet<Comment> Comments => Set<Comment>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Define your DbSets (tables) here
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(x => x.RoleId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // UserAccount
            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Email).IsUnique();
                entity.HasOne(x => x.User)
                      .WithOne(u => u.UserAccount)
                      .HasForeignKey<UserAccount>(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            // Post
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Slug).IsUnique();
                entity.HasOne(x => x.Author)
                      .WithMany(u => u.Posts)
                      .HasForeignKey(x => x.AuthorId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(x => x.Category)
                      .WithMany(c => c.Posts)
                      .HasForeignKey(x => x.CategoryId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Slug).IsUnique();
            });

            // Tag
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Slug).IsUnique();
            });

            // PostTag (many-to-many)
            modelBuilder.Entity<PostTag>(entity =>
            {
                entity.HasKey(pt => new { pt.PostId, pt.TagId });
                entity.HasOne(pt => pt.Post)
                      .WithMany(p => p.PostTags)
                      .HasForeignKey(pt => pt.PostId);
                entity.HasOne(pt => pt.Tag)
                      .WithMany(t => t.PostTags)
                      .HasForeignKey(pt => pt.TagId);
            });

            // Comment (self reference)
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Post)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(x => x.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Author)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(x => x.AuthorId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(x => x.Parent)
                      .WithMany(p => p.Replies)
                      .HasForeignKey(x => x.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

