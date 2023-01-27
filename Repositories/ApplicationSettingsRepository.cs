using Microsoft.EntityFrameworkCore;
using ShopifyInventorySync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.Repositories
{
    public class ApplicationSettingsRepository : IApplicationSettingsRepository
    {
        private readonly EFDbContext shopifyDbContext;
        private bool disposed = false;

        public ApplicationSettingsRepository()
        {
            shopifyDbContext = new()!;
        }

        public ApplicationSettingsRepository(EFDbContext context)
        {
            shopifyDbContext = context;
        }
        public IEnumerable<ApplicationSetting> GetAll()
        {
            try
            {
                return shopifyDbContext.ApplicationSettings.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public ApplicationSetting GetById(int Id)
        {
            try
            {
                return shopifyDbContext.ApplicationSettings.Find(Id)!;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Insert(ApplicationSetting applicationSetting)
        {
            try
            {
                shopifyDbContext.ApplicationSettings.Add(applicationSetting);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Update(ApplicationSetting applicationSetting)
        {
            try
            {
                shopifyDbContext.Entry(applicationSetting).State = EntityState.Modified;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Delete(int Id)
        {
            try
            {
                ApplicationSetting applicationSetting = shopifyDbContext.ApplicationSettings.Find(Id)!;

                shopifyDbContext.ApplicationSettings.Remove(applicationSetting!);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Save()
        {
            try
            {
                shopifyDbContext.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    shopifyDbContext.Dispose();
                }
            }

            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}
