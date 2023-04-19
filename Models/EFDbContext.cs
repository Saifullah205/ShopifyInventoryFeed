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
        public virtual DbSet<RestrictedTerm> RestrictedTerms { get; set; } = null!;
        public virtual DbSet<ShopifyFixedPricesBkp20230312> ShopifyFixedPricesBkp20230312s { get; set; } = null!;
        public virtual DbSet<ShopifyInventoryDatum> ShopifyInventoryData { get; set; } = null!;
        public virtual DbSet<WalmartFeedResponse> WalmartFeedResponses { get; set; } = null!;
        public virtual DbSet<WalmartInventoryDatum> WalmartInventoryData { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=(LocalDB)\\MSSQLLocalDB;Database=" + Environment.CurrentDirectory + "\\ShopifyInventory.mdf;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationSetting>(entity =>
            {
                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsVisible).HasDefaultValueSql("((0))");

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
                    .HasConstraintName("FK__FixedPric__EComS__57DD0BE4");
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
                    .HasConstraintName("FK__MarkUpPri__EComS__531856C7");
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
                    .HasConstraintName("FK__Restricte__EComS__51300E55");
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
                    .HasConstraintName("FK__Restricte__EComS__5224328E");
            });

            modelBuilder.Entity<RestrictedTerm>(entity =>
            {
                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ApiType)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.EcomStoreId).HasColumnName("EComStoreId");

                entity.Property(e => e.Term)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.EcomStore)
                    .WithMany(p => p.RestrictedTerms)
                    .HasForeignKey(d => d.EcomStoreId)
                    .HasConstraintName("FK__Restricte__EComS__09746778");
            });

            modelBuilder.Entity<ShopifyFixedPricesBkp20230312>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("ShopifyFixedPrices_bkp_20230312");

                entity.Property(e => e.AddDate).HasColumnType("datetime");

                entity.Property(e => e.ApiType)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.FixedPrice)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

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

                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

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

            modelBuilder.Entity<WalmartFeedResponse>(entity =>
            {
                entity.ToTable("WalmartFeedResponse");

                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EcomStoreId).HasColumnName("EComStoreId");

                entity.Property(e => e.FeedId)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .HasColumnName("FeedID");

                entity.Property(e => e.FeedType)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.HasOne(d => d.EcomStore)
                    .WithMany(p => p.WalmartFeedResponses)
                    .HasForeignKey(d => d.EcomStoreId)
                    .HasConstraintName("FK__WalmartFe__EComS__671F4F74");
            });

            modelBuilder.Entity<WalmartInventoryDatum>(entity =>
            {
                entity.Property(e => e.AddDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.BrandName)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.IsShippingMapped).HasDefaultValueSql("((0))");

                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Sku)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.SkuPrefix)
                    .HasMaxLength(3)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
