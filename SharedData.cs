using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync
{
    public static class SharedData
    {
        public enum APIType
        {
            FragranceX,
            CSV
        } 
    }

    public static class SharedFunctions
    {
        private static readonly object lockerFile = new Object();

        public static DataTable LinqToDataTable<T>(IEnumerable<T> items)
        {
            //Createa DataTable with the Name of the Class i.e. Customer class.
            DataTable dt = new DataTable(typeof(T).Name);

            //Read all the properties of the Class i.e. Customer class.
            PropertyInfo[] propInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            //Loop through each property of the Class i.e. Customer class.
            foreach (PropertyInfo propInfo in propInfos)
            {
                //Add Columns in DataTable based on Property Name and Type.
                dt.Columns.Add(new DataColumn(propInfo.Name, Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType));
            }

            //Loop through the items if the Collection.
            foreach (T item in items)
            {
                //Add a new Row to DataTable.
                DataRow dr = dt.Rows.Add();

                //Loop through each property of the Class i.e. Customer class.
                foreach (PropertyInfo propInfo in propInfos)
                {
                    //Add value Column to the DataRow.
                    dr[propInfo.Name] = propInfo.GetValue(item, null);
                }
            }

            return dt;
        }

        public static void logErrorToFile(Exception ex)
        {
            WriteToErrorLog(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace);
        }

        public static void WriteToErrorLog(String text)
        {
            try
            {
                string fileName = "ExceptionLog-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

                lock (lockerFile)
                {
                    if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "ErrorLog")))
                    {
                        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "ErrorLog"));

                        if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "ErrorLog", fileName)))
                        {
                            File.Create(Path.Combine(Environment.CurrentDirectory, "ErrorLog", fileName));
                        }
                    }
                
                    File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "ErrorLog", fileName), text + Environment.NewLine);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
