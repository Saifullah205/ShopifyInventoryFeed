using CsvHelper;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.BusinessLogic
{
    internal class FragranceNetAPI
    {

        ApplicationState applicationState;

        private readonly IProductsRepository productsRepository;

        public FragranceNetAPI(IProductsRepository productsRepository)
        {
            applicationState = ApplicationState.GetState;

            this.productsRepository = productsRepository;
        }

        public FragranceNetProductsList GetDataFromSource()
        {
            string fileTextData;
            string csvTextData;
            FragranceNetProductsList fragranceNetProducts = new();
            byte[] csvBytes;

            try
            {
                fileTextData = File.ReadAllText("C:\\Clients_Work\\20220827_tdog5116\\FragranceNet.txt");
                
                fileTextData = fileTextData.Replace("\"","");
                fileTextData = "\"" + fileTextData;
                fileTextData = fileTextData.Replace("\t", "\",\"");
                csvTextData = fileTextData.Replace("\r\n", "\"\r\n\"");

                csvTextData = csvTextData.Substring(0, csvTextData.Length - 1);

                csvBytes = Encoding.UTF8.GetBytes(csvTextData);

                using (var reader = new StreamReader(new MemoryStream(csvBytes)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    fragranceNetProducts.products = csv.GetRecords<FragranceNetProduct>().ToList<FragranceNetProduct>();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return fragranceNetProducts;
        }

        public List<ShopifyInventoryDatum> FilterRemovedProducts(FragranceNetProductsList fragranceNetProducts)
        {
            List<ShopifyInventoryDatum> shopifyProductsToRemove = new List<ShopifyInventoryDatum>();
            List<FragranceNetProduct> products = new List<FragranceNetProduct>();

            try
            {
                products = fragranceNetProducts.products;

                shopifyProductsToRemove = (from s in this.productsRepository.GetBySkuPrefix(GlobalConstants.fragranceNetSKUPrefix)
                                           where !products.Any(x => x.sku == s.Sku && s.SkuPrefix == GlobalConstants.fragranceNetSKUPrefix)
                                           select s).ToList<ShopifyInventoryDatum>();
            }
            catch (Exception)
            {
                throw;
            }

            return shopifyProductsToRemove;
        }

        public void FormatSourceProductsData(FragranceNetProductsList fragranceNetProducts)
        {

        }
    }
}
