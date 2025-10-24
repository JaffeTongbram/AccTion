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

    public virtual DbSet<SubscriptionType> SubscriptionTypes { get; set; }

    public virtual DbSet<UserTable> UserTables { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=newwebapp;Password=newwebapp@123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            entity.Property(e => e.SubscriptionTypeId).HasColumnName("subscription_type_id");
            entity.Property(e => e.UserTypeId).HasColumnName("user_type_id");
            entity.Property(e => e.PhotoPath)
                .HasMaxLength(500)
                .HasColumnName("PhotoPath");
            entity.Property(e => e.PhotoData).HasColumnName("PhotoData");

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
