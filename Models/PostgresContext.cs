using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AccTion.Models;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Interaction> Interactions { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Subscriber> Subscribers { get; set; }

    public virtual DbSet<SubscriptionType> SubscriptionTypes { get; set; }

    public virtual DbSet<UserTable> UserTables { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=newwebapp;Password=newwebapp@123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Interaction>(entity =>
        {
            entity.HasKey(e => e.InteractionId).HasName("interactions_pkey");

            entity.ToTable("interactions");

            entity.Property(e => e.InteractionId).HasColumnName("interaction_id");
            entity.Property(e => e.Commentnum).HasColumnName("commentnum");
            entity.Property(e => e.InteractionUserId).HasColumnName("interaction_user_id");
            entity.Property(e => e.Likenum).HasColumnName("likenum");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.CommentText).HasColumnName("comment_text"); // ✅ ADD THIS
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .HasColumnName("created_at"); 

            // entity.HasOne(d => d.InteractionUser).WithMany(p => p.Interactions)
            //     .HasForeignKey(d => d.InteractionUserId)
            //     .HasConstraintName("fk_interaction_user");

            // entity.HasOne(d => d.Post).WithMany(p => p.Interactions)
            //     .HasForeignKey(d => d.PostId)
            //     .HasConstraintName("fk_interaction_post");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("post_pkey");

            entity.ToTable("post");

            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.Caption)
                .HasMaxLength(500)
                .HasColumnName("caption");
            entity.Property(e => e.CommentCount)
                .HasDefaultValue(0)
                .HasColumnName("comment_count");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .HasColumnName("image");
            entity.Property(e => e.LikeCount)
                .HasDefaultValue(0)
                .HasColumnName("like_count");
            entity.Property(e => e.SubPermission)
                .HasMaxLength(10)
                .HasDefaultValueSql("'Public'::character varying")
                .HasColumnName("sub_permission");
            entity.Property(e => e.UserTableId).HasColumnName("user_table_id");
            entity.Property(e => e.Video)
                .HasMaxLength(255)
                .HasColumnName("video");

            // entity.HasOne(d => d.UserTable).WithMany(p => p.Posts)
            //     .HasForeignKey(d => d.UserTableId)
            //     .HasConstraintName("fk_post_user");
        });

        modelBuilder.Entity<Subscriber>(entity =>
        {
            entity.HasKey(e => e.SubId).HasName("subscriber_pkey");

            entity.ToTable("subscriber");

            entity.HasIndex(e => new { e.SubscribeBy, e.SubscribeTo }, "uq_subscription").IsUnique();

            entity.Property(e => e.SubId).HasColumnName("sub_id");
            entity.Property(e => e.SubscribeBy).HasColumnName("subscribe_by");
            entity.Property(e => e.SubscribeTo).HasColumnName("subscribe_to");

            // entity.HasOne(d => d.SubscribeByNavigation).WithMany(p => p.SubscriberSubscribeByNavigations)
            //     .HasForeignKey(d => d.SubscribeBy)
            //     .HasConstraintName("fk_sub_by");

            // entity.HasOne(d => d.SubscribeToNavigation).WithMany(p => p.SubscriberSubscribeToNavigations)
            //     .HasForeignKey(d => d.SubscribeTo)
            //     .HasConstraintName("fk_sub_to");
        });

        modelBuilder.Entity<SubscriptionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscription_type_pkey");

            entity.ToTable("subscription_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Level)
                .HasMaxLength(50)
                .HasColumnName("level");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("start_date");
            entity.Property(e => e.Validity).HasColumnName("validity");
        });

        modelBuilder.Entity<UserTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_table_pkey");

            entity.ToTable("user_table");

            entity.HasIndex(e => e.Email, "user_table_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PNo)
                .HasMaxLength(15)
                .HasColumnName("p_no");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("password");
            entity.Property(e => e.PhotoPath).HasMaxLength(255);
            entity.Property(e => e.SubscriptionTypeId).HasColumnName("subscription_type_id");
            entity.Property(e => e.UserTypeId).HasColumnName("user_type_id");

            // entity.HasOne(d => d.SubscriptionType).WithMany(p => p.UserTables)
            //     .HasForeignKey(d => d.SubscriptionTypeId)
            //     .OnDelete(DeleteBehavior.SetNull)
            //     .HasConstraintName("fk_user_subscription_type");

            // entity.HasOne(d => d.UserType).WithMany(p => p.UserTables)
            //     .HasForeignKey(d => d.UserTypeId)
            //     .OnDelete(DeleteBehavior.SetNull)
            //     .HasConstraintName("user_table_user_type_id_fkey");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_type_pkey");

            entity.ToTable("user_type");

            entity.HasIndex(e => e.TypeName, "user_type_type_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("type_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
