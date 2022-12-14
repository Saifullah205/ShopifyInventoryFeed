using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ShopifyInventorySync.Models
{
    public partial class ShopifyDbContext : DbContext
    {
        public ShopifyDbContext()
        {
        }

        public ShopifyDbContext(DbContextOptions<ShopifyDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ApplicationSetting> ApplicationSettings { get; set; } = null!;
        public virtual DbSet<ClientApi> ClientApis { get; set; } = null!;
        public virtual DbSet<MarkUpPrice> MarkUpPrices { get; set; } = null!;
        public virtual DbSet<RestrictedBrand> RestrictedBrands { get; set; } = null!;
        public virtual DbSet<RestrictedSku> RestrictedSkus { get; set; } = null!;
        public virtual DbSet<ShopifyFixedPrice> ShopifyFixedPrices { get; set; } = null!;
        public virtual DbSet<ShopifyInventoryDatum> ShopifyInventoryData { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=" + Environment.CurrentDirectory + "\\ShopifyInventory.mdf;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationSetting>(entity =>
            {
                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Tag)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.TagValue)
                    .HasMaxLength(4000)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ClientApi>(entity =>
            {
                entity.Property(e => e.ApiDescription)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ApiType)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<MarkUpPrice>(entity =>
            {
                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.MarkupPercentage).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.MaxPrice).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.MinPrice).HasColumnType("decimal(18, 0)");
            });

            modelBuilder.Entity<RestrictedBrand>(entity =>
            {
                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ApiType)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.BrandName)
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RestrictedSku>(entity =>
            {
                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Sku)
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ShopifyFixedPrice>(entity =>
            {
                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FixedPrice)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Sku)
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ShopifyInventoryDatum>(entity =>
            {
                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.BrandName)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ImageId)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("ImageID");

                entity.Property(e => e.InventoryItemId)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("InventoryItemID");

                entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsDisabled).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsOutOfStock).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsRestricted).HasDefaultValueSql("((0))");

                entity.Property(e => e.ProductGender)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProductName)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.ShopifyId)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Sku)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SkuPrefix)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.VariantId)
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
