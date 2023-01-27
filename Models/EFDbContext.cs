using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ShopifyInventorySync.Models
{
    public partial class EFDbContext : DbContext
    {
        public EFDbContext()
        {
        }

        public EFDbContext(DbContextOptions<EFDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ApplicationSetting> ApplicationSettings { get; set; } = null!;
        public virtual DbSet<ClientApi> ClientApis { get; set; } = null!;
        public virtual DbSet<EcomStore> EcomStores { get; set; } = null!;
        public virtual DbSet<FixedPrice> FixedPrices { get; set; } = null!;
        public virtual DbSet<MarkUpPrice> MarkUpPrices { get; set; } = null!;
        public virtual DbSet<RestrictedBrand> RestrictedBrands { get; set; } = null!;
        public virtual DbSet<RestrictedSku> RestrictedSkus { get; set; } = null!;
        public virtual DbSet<ShopifyInventoryDatum> ShopifyInventoryData { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=(LocalDB)\\MSSQLLocalDB;Database=D:\\Clients_Work\\20220827_tdog5116\\SourceCodes\\ShopifyInventoryFeed\\bin\\Debug\\net6.0-windows\\ShopifyInventory.mdf;Trusted_Connection=True;");
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

            modelBuilder.Entity<EcomStore>(entity =>
            {
                entity.ToTable("EComStores");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.StoreName)
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FixedPrice>(entity =>
            {
                entity.Property(e => e.ApiType)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EcomStoreId).HasColumnName("EComStoreId");

                entity.Property(e => e.FixPrice)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Sku)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.HasOne(d => d.EcomStore)
                    .WithMany(p => p.FixedPrices)
                    .HasForeignKey(d => d.EcomStoreId)
                    .HasConstraintName("FK__FixedPric__EComS__5E8A0973");
            });

            modelBuilder.Entity<MarkUpPrice>(entity =>
            {
                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ApiType)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.EcomStoreId).HasColumnName("EComStoreId");

                entity.Property(e => e.MarkupPercentage).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.MaxPrice).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.MinPrice).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.EcomStore)
                    .WithMany(p => p.MarkUpPrices)
                    .HasForeignKey(d => d.EcomStoreId)
                    .HasConstraintName("FK__MarkUpPri__EComS__5224328E");
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

                entity.Property(e => e.EcomStoreId).HasColumnName("EComStoreId");

                entity.HasOne(d => d.EcomStore)
                    .WithMany(p => p.RestrictedBrands)
                    .HasForeignKey(d => d.EcomStoreId)
                    .HasConstraintName("FK__Restricte__EComS__503BEA1C");
            });

            modelBuilder.Entity<RestrictedSku>(entity =>
            {
                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ApiType)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.EcomStoreId).HasColumnName("EComStoreId");

                entity.Property(e => e.Sku)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.HasOne(d => d.EcomStore)
                    .WithMany(p => p.RestrictedSkus)
                    .HasForeignKey(d => d.EcomStoreId)
                    .HasConstraintName("FK__Restricte__EComS__51300E55");
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
