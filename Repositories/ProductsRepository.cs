using Microsoft.EntityFrameworkCore;
using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly ShopifyDbContext shopifyDbContext;
        private bool disposed = false;

        public ProductsRepository()
        {
            shopifyDbContext = new()!;
        }

        public ProductsRepository(ShopifyDbContext context)
        {
            shopifyDbContext = context;
        }

        public void Delete(string Id)
        {
            try
            {
                List<ShopifyInventoryDatum> products = shopifyDbContext.ShopifyInventoryData.Where(m => m.ShopifyId == Id).ToList();

                shopifyDbContext.ShopifyInventoryData.RemoveRange(products);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteMultiple(List<ShopifyInventoryDatum> products)
        {
            try
            {
                shopifyDbContext.ShopifyInventoryData.RemoveRange(products);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<ShopifyInventoryDatum> GetAll()
        {
            try
            {
                return shopifyDbContext.ShopifyInventoryData.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ShopifyInventoryDatum GetById(string Id)
        {
            try
            {
                return shopifyDbContext.ShopifyInventoryData.Where(m => m.ShopifyId == Id).FirstOrDefault()!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<ShopifyInventoryDatum> GetBySkuPrefix(string skuPrefix)
        {
            try
            {
                return shopifyDbContext.ShopifyInventoryData.Where(m=>m.SkuPrefix == skuPrefix).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Insert(ShopifyInventoryDatum product)
        {
            try
            {
                shopifyDbContext.ShopifyInventoryData.Add(product);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertMultiple(List<ShopifyInventoryDatum> products)
        {
            try
            {
                shopifyDbContext.ShopifyInventoryData.AddRange(products);
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

        public void Update(ShopifyInventoryDatum product)
        {
            try
            {
                shopifyDbContext.Update(product);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateMultiple(List<ShopifyInventoryDatum> products)
        {
            try
            {
                shopifyDbContext.UpdateRange(products);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
