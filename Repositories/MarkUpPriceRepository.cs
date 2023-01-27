using Microsoft.EntityFrameworkCore;
using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public class MarkUpPriceRepository : IMarkUpPriceRepository
    {
        private readonly EFDbContext shopifyDbContext;
        private bool disposed = false;

        public MarkUpPriceRepository()
        {
            shopifyDbContext = new()!;
        }

        public MarkUpPriceRepository(EFDbContext context)
        {
            shopifyDbContext = context;
        }

        public IEnumerable<MarkUpPrice> GetAll()
        {
            try
            {
                return shopifyDbContext.MarkUpPrices.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<MarkUpPrice> GetByClientAPI(string ClientAPI)
        {
            try
            {
                return shopifyDbContext.MarkUpPrices.Where(m => m.ApiType == ClientAPI).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public MarkUpPrice GetById(int Id)
        {
            try
            {
                return shopifyDbContext.MarkUpPrices.Find(Id)!;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Insert(MarkUpPrice markUpPrice)
        {
            try
            {
                shopifyDbContext.MarkUpPrices.Add(markUpPrice);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Update(MarkUpPrice markUpPrice)
        {
            try
            {
                shopifyDbContext.Entry(markUpPrice).State = EntityState.Modified;
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
                MarkUpPrice markUpPrice = shopifyDbContext.MarkUpPrices.Find(Id)!;

                shopifyDbContext.MarkUpPrices.Remove(markUpPrice!);
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
