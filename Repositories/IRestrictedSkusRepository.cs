using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public interface IRestrictedSkusRepository
    {
        IEnumerable<RestrictedSku> GetAll();
        IEnumerable<RestrictedSku> GetByClientAPI(string ClientAPI);
        RestrictedSku GetById(int Id);
        void Insert(RestrictedSku restrictedSku);
        void Update(RestrictedSku restrictedSku);
        void Delete(int Id);
        void Save();
    }
}
