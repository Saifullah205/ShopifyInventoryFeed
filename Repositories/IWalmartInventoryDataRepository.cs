using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public interface IWalmartInventoryDataRepository
    {
        IEnumerable<WalmartInventoryDatum> GetAll();
        IEnumerable<WalmartInventoryDatum> GetBySkuPrefix(string skuPrefix);
        WalmartInventoryDatum GetById(int Id);
        void Insert(WalmartInventoryDatum product);
        void InsertMultiple(List<WalmartInventoryDatum> products);
        void Update(WalmartInventoryDatum product);
        void UpdateMultiple(List<WalmartInventoryDatum> products);
        void Delete(string Id);
        void DeleteMultiple(List<WalmartInventoryDatum> products);
        void Save();
    }
}
