using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public interface IRestrictedBrandsRepository
    {
        IEnumerable<RestrictedBrand> GetAll();
        IEnumerable<RestrictedBrand> GetByClientAPI(string ClientAPI);
        RestrictedBrand GetById(int Id);
        void Insert(RestrictedBrand applicationSetting);
        void Update(RestrictedBrand applicationSetting);
        void Delete(int Id);
        void Save();
    }
}
