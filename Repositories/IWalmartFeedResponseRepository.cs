using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public interface IWalmartFeedResponseRepository
    {
        IEnumerable<WalmartFeedResponse> GetAll();
        WalmartFeedResponse GetById(int Id);
        void Insert(WalmartFeedResponse walmartFeedResponse);
        void InsertMultiple(List<WalmartFeedResponse> products);
        void Update(WalmartFeedResponse walmartFeedResponse);
        void UpdateMultiple(List<WalmartFeedResponse> products);
        void Delete(int Id);
        void DeleteMultiple(List<WalmartFeedResponse> products);
        void Save();
    }
}
