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
        IEnumerable<FixedPrice> GetAll();
        IEnumerable<FixedPrice> GetByClientAPI(string ClientAPI);
        FixedPrice GetById(int Id);
        void Insert(FixedPrice shopifyFixedPrice);
        void Update(FixedPrice shopifyFixedPrice);
        void Delete(int Id);
        void Save();
    }
}
