using Microsoft.EntityFrameworkCore;
using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public class FixedPriceRespsitory : IFixedPriceRespsitory
    {
        private readonly ShopifyDbContext shopifyDbContext;
        private bool disposed = false;

        public FixedPriceRespsitory()
        {
            shopifyDbContext = new()!;
        }

        public FixedPriceRespsitory(ShopifyDbContext context)
        {
            shopifyDbContext = context;
        }

        public IEnumerable<ShopifyFixedPrice> GetAll()
        {
            try
            {
                return shopifyDbContext.ShopifyFixedPrices.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<ShopifyFixedPrice> GetByClientAPI(string ClientAPI)
        {
            try
            {
                return shopifyDbContext.ShopifyFixedPrices.Where(m => m.ApiType == ClientAPI).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ShopifyFixedPrice GetById(int Id)
        {
            try
            {
                return shopifyDbContext.ShopifyFixedPrices.Find(Id)!;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Insert(ShopifyFixedPrice shopifyFixedPrice)
        {
            try
            {
                shopifyDbContext.ShopifyFixedPrices.Add(shopifyFixedPrice);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Update(ShopifyFixedPrice shopifyFixedPrice)
        {
            try
            {
                shopifyDbContext.Entry(shopifyFixedPrice).State = EntityState.Modified;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Delete(int Id)
        {
            try
            {
                ShopifyFixedPrice shopifyFixedPrice = shopifyDbContext.ShopifyFixedPrices.Find(Id)!;

                shopifyDbContext.ShopifyFixedPrices.Remove(shopifyFixedPrice!);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Save()
        {
            try
            {
                shopifyDbContext.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    shopifyDbContext.Dispose();
                }
            }

            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}
