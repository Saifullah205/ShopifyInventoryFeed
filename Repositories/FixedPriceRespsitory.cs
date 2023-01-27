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
        private readonly EFDbContext shopifyDbContext;
        private bool disposed = false;

        public FixedPriceRespsitory()
        {
            shopifyDbContext = new()!;
        }

        public FixedPriceRespsitory(EFDbContext context)
        {
            shopifyDbContext = context;
        }

        public IEnumerable<FixedPrice> GetAll()
        {
            try
            {
                return shopifyDbContext.FixedPrices.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<FixedPrice> GetByClientAPI(string ClientAPI)
        {
            try
            {
                return shopifyDbContext.FixedPrices.Where(m => m.ApiType == ClientAPI).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public FixedPrice GetById(int Id)
        {
            try
            {
                return shopifyDbContext.FixedPrices.Find(Id)!;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Insert(FixedPrice shopifyFixedPrice)
        {
            try
            {
                shopifyDbContext.FixedPrices.Add(shopifyFixedPrice);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Update(FixedPrice shopifyFixedPrice)
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
                FixedPrice shopifyFixedPrice = shopifyDbContext.FixedPrices.Find(Id)!;

                shopifyDbContext.FixedPrices.Remove(shopifyFixedPrice!);
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
