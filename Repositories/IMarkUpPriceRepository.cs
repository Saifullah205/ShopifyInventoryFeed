using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public interface IMarkUpPriceRepository
    {
        IEnumerable<MarkUpPrice> GetAll();
        IEnumerable<MarkUpPrice> GetByClientAPI(string ClientAPI);
        MarkUpPrice GetById(int Id);
        void Insert(MarkUpPrice markUpPrice);
        void Update(MarkUpPrice markUpPrice);
        void Delete(int Id);
        void Save();
    }
}
