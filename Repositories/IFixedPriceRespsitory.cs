using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public interface IFixedPriceRespsitory
    {
        IEnumerable<ShopifyFixedPrice> GetAll();
        IEnumerable<ShopifyFixedPrice> GetByClientAPI(string ClientAPI);
        ShopifyFixedPrice GetById(int Id);
        void Insert(ShopifyFixedPrice shopifyFixedPrice);
        void Update(ShopifyFixedPrice shopifyFixedPrice);
        void Delete(int Id);
        void Save();
    }
}
