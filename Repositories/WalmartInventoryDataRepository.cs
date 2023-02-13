using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    internal class WalmartInventoryDataRepository : IWalmartInventoryDataRepository
    {
        private readonly EFDbContext walmartDbContext;
        private bool disposed = false;

        public WalmartInventoryDataRepository()
        {
            walmartDbContext = new()!;
        }

        public WalmartInventoryDataRepository(EFDbContext context)
        {
            walmartDbContext = context;
        }

        public void Delete(string sku)
        {
            try
            {
                List<WalmartInventoryDatum> products = walmartDbContext.WalmartInventoryData.Where(m => m.Sku == sku).ToList();

                walmartDbContext.WalmartInventoryData.RemoveRange(products);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteMultiple(List<WalmartInventoryDatum> products)
        {
            try
            {
                walmartDbContext.WalmartInventoryData.RemoveRange(products);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<WalmartInventoryDatum> GetAll()
        {
            try
            {
                return walmartDbContext.WalmartInventoryData.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public WalmartInventoryDatum GetById(int Id)
        {
            try
            {
                return walmartDbContext.WalmartInventoryData.Where(m => m.Id == Id).FirstOrDefault()!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<WalmartInventoryDatum> GetBySkuPrefix(string skuPrefix)
        {
            try
            {
                return walmartDbContext.WalmartInventoryData.Where(m => m.SkuPrefix == skuPrefix).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Insert(WalmartInventoryDatum product)
        {
            try
            {
                walmartDbContext.WalmartInventoryData.Add(product);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertMultiple(List<WalmartInventoryDatum> products)
        {
            try
            {
                walmartDbContext.WalmartInventoryData.AddRange(products);
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

        public void Update(WalmartInventoryDatum product)
        {
            try
            {
                walmartDbContext.Update(product);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateMultiple(List<WalmartInventoryDatum> products)
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
    }
}
