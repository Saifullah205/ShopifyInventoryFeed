using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public interface IRestrictedTermsRepository
    {
        IEnumerable<RestrictedTerm> GetAll();
        IEnumerable<RestrictedTerm> GetByClientAPI(string ClientAPI);
        RestrictedTerm GetById(int Id);
        void Insert(RestrictedTerm restrictedTerm);
        void Update(RestrictedTerm restrictedTerm);
        void Delete(int Id);
        void Save();
    }
}
