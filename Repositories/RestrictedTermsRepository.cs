using Microsoft.EntityFrameworkCore;
using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public class RestrictedTermsRepository : IRestrictedTermsRepository
    {
        private readonly EFDbContext shopifyDbContext;
        private bool disposed = false;

        public RestrictedTermsRepository()
        {
            shopifyDbContext = new()!;
        }

        public RestrictedTermsRepository(EFDbContext context)
        {
            shopifyDbContext = context;
        }

        public IEnumerable<RestrictedTerm> GetAll()
        {
            try
            {
                return shopifyDbContext.RestrictedTerms.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<RestrictedTerm> GetByClientAPI(string ClientAPI)
        {
            try
            {
                return shopifyDbContext.RestrictedTerms.Where(m => m.ApiType == ClientAPI).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public RestrictedTerm GetById(int Id)
        {
            try
            {
                return shopifyDbContext.RestrictedTerms.Find(Id)!;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Insert(RestrictedTerm restrictedTerm)
        {
            try
            {
                shopifyDbContext.RestrictedTerms.Add(restrictedTerm);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Update(RestrictedTerm restrictedTerm)
        {
            try
            {
                shopifyDbContext.Entry(restrictedTerm).State = EntityState.Modified;
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
                RestrictedTerm restrictedTerm = shopifyDbContext.RestrictedTerms.Find(Id)!;

                shopifyDbContext.RestrictedTerms.Remove(restrictedTerm!);
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
