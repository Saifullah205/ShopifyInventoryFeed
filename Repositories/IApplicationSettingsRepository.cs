using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public interface IApplicationSettingsRepository
    {
        IEnumerable<ApplicationSetting> GetAll();
        ApplicationSetting GetById(int Id);
        void Insert(ApplicationSetting applicationSetting);
        void Update(ApplicationSetting applicationSetting);
        void Delete(int Id);
        void Save();
    }
}
