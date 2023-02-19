using Microsoft.EntityFrameworkCore;
using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public class WalmartFeedResponseRepository : IWalmartFeedResponseRepository
    {
        private readonly EFDbContext walmartDbContext;
        private bool disposed = false;

        public WalmartFeedResponseRepository()
        {
            walmartDbContext = new()!;
        }

        public WalmartFeedResponseRepository(EFDbContext context)
        {
            walmartDbContext = context;
        }

        public IEnumerable<WalmartFeedResponse> GetAll()
        {
            try
            {
                return walmartDbContext.WalmartFeedResponses.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public WalmartFeedResponse GetById(int Id)
        {
            try
            {
                return walmartDbContext.WalmartFeedResponses.Find(Id)!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Insert(WalmartFeedResponse walmartFeedResponse)
        {
            try
            {
                walmartDbContext.WalmartFeedResponses.Add(walmartFeedResponse);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertMultiple(List<WalmartFeedResponse> products)
        {
            try
            {
                walmartDbContext.WalmartFeedResponses.AddRange(products);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Update(WalmartFeedResponse walmartFeedResponse)
        {
            try
            {
                walmartDbContext.Entry(walmartFeedResponse).State = EntityState.Modified;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateMultiple(List<WalmartFeedResponse> products)
        {
            try
            {
                walmartDbContext.UpdateRange(products);
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
                WalmartFeedResponse walmartFeedResponse = walmartDbContext.WalmartFeedResponses.Find(Id)!;

                walmartDbContext.WalmartFeedResponses.Remove(walmartFeedResponse!);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteMultiple(List<WalmartFeedResponse> products)
        {
            try
            {
                walmartDbContext.WalmartFeedResponses.RemoveRange(products);
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
                walmartDbContext.SaveChanges();
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
                    walmartDbContext.Dispose();
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
