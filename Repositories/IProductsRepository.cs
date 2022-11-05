using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public interface IProductsRepository
    {
        IEnumerable<ShopifyInventoryDatum> GetAll();
        IEnumerable<ShopifyInventoryDatum> GetBySkuPrefix(string skuPrefix);
        ShopifyInventoryDatum GetById(string Id);
        void Insert(ShopifyInventoryDatum product);
        void InsertMultiple(List<ShopifyInventoryDatum> products);
        void Update(ShopifyInventoryDatum product);
        void UpdateMultiple(List<ShopifyInventoryDatum> products);
        void Delete(string Id);
        void DeleteMultiple(List<ShopifyInventoryDatum> products);
        void Save();
    }
}
