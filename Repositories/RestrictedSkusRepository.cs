using Microsoft.EntityFrameworkCore;
using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public class RestrictedSkusRepository : IRestrictedSkusRepository
    {
        private readonly ShopifyDbContext shopifyDbContext;
        private bool disposed = false;

        public RestrictedSkusRepository()
        {
            shopifyDbContext = new()!;
        }

        public RestrictedSkusRepository(ShopifyDbContext context)
        {
            shopifyDbContext = context;
        }

        public IEnumerable<RestrictedSku> GetAll()
        {
            try
            {
                return shopifyDbContext.RestrictedSkus.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<RestrictedSku> GetByClientAPI(string ClientAPI)
        {
            try
            {
                return shopifyDbContext.RestrictedSkus.Where(m => m.ApiType == ClientAPI).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public RestrictedSku GetById(int Id)
        {
            try
            {
                return shopifyDbContext.RestrictedSkus.Find(Id)!;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Insert(RestrictedSku restrictedSku)
        {
            try
            {
                shopifyDbContext.RestrictedSkus.Add(restrictedSku);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Update(RestrictedSku restrictedSku)
        {
            try
            {
                shopifyDbContext.Entry(restrictedSku).State = EntityState.Modified;
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
                RestrictedSku restrictedSku = shopifyDbContext.RestrictedSkus.Find(Id)!;

                shopifyDbContext.RestrictedSkus.Remove(restrictedSku!);
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
