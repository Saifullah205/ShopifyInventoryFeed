using Microsoft.EntityFrameworkCore;
using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public class RestrictedBrandsRepository : IRestrictedBrandsRepository
    {
        private readonly ShopifyDbContext shopifyDbContext;
        private bool disposed = false;

        public RestrictedBrandsRepository()
        {
            shopifyDbContext = new()!;
        }

        public RestrictedBrandsRepository(ShopifyDbContext context)
        {
            shopifyDbContext = context;
        }

        public IEnumerable<RestrictedBrand> GetAll()
        {
            try
            {
                return shopifyDbContext.RestrictedBrands.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<RestrictedBrand> GetByClientAPI(string ClientAPI)
        {
            try
            {
                return shopifyDbContext.RestrictedBrands.Where(m => m.ApiType == ClientAPI).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public RestrictedBrand GetById(int Id)
        {
            try
            {
                return shopifyDbContext.RestrictedBrands.Find(Id)!;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Insert(RestrictedBrand restrictedBrand)
        {
            try
            {
                shopifyDbContext.RestrictedBrands.Add(restrictedBrand);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Update(RestrictedBrand restrictedBrand)
        {
            try
            {
                shopifyDbContext.Entry(restrictedBrand).State = EntityState.Modified;
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
                RestrictedBrand restrictedBrand = shopifyDbContext.RestrictedBrands.Find(Id)!;

                shopifyDbContext.RestrictedBrands.Remove(restrictedBrand!);
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
