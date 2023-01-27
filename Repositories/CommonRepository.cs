using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public class CommonRepository : ICommonRepository
    {
        private readonly EFDbContext shopifyDbContext;
        private bool disposed = false;

        public CommonRepository()
        {
            shopifyDbContext = new()!;
        }

        public List<ClientApi> GetClientApis()
        {
            try
            {
                return shopifyDbContext.ClientApis.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CommonRepository(EFDbContext context)
        {
            shopifyDbContext = context;
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
