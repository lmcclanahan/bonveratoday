using Exigo.Api;
using Exigo.Api.Base;
using Exigo.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api
{
    public class ItemService : IItemService
    {
        public IItemService Provider { get; set; }
        public ItemService(IItemService Provider = null)
        {
            if (Provider != null)
            {
                this.Provider = Provider;
            }
            else
            {
                var defaultApiSettings = new DefaultApiSettings();
                if(defaultApiSettings.IsEnterprise) this.Provider = new SqlItemProvider(defaultApiSettings);
                else this.Provider = new ODataItemProvider(defaultApiSettings);
            }
        }

        public PriceType GetPriceType(int PriceTypeID)
        {
            return Provider.GetPriceType(PriceTypeID);
        }
        public List<PriceType> GetPriceTypes()
        {
            return Provider.GetPriceTypes();
        }

        public Warehouse GetWarehouse(int WarehouseID)
        {
            return Provider.GetWarehouse(WarehouseID);
        }
        public List<Warehouse> GetWarehouses()
        {
            return Provider.GetWarehouses();
        }

        public Language GetLanguage(int LanguageID)
        {
            return Provider.GetLanguage(LanguageID);
        }
        public List<Language> GetLanguages()
        {
            return Provider.GetLanguages();
        }
    }

    public interface IItemService
    {
        PriceType GetPriceType(int PriceTypeID);
        List<PriceType> GetPriceTypes();

        Warehouse GetWarehouse(int WarehouseID);
        List<Warehouse> GetWarehouses();

        Language GetLanguage(int LanguageID);
        List<Language> GetLanguages();
    }

    #region Web Service
    public class WebServiceItemProvider : BaseWebServiceProvider, IItemService
    {
        public WebServiceItemProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public PriceType GetPriceType(int PriceTypeID)
        {
            if(ApiSettings.IsEnterprise) return new SqlItemProvider(ApiSettings).GetPriceType(PriceTypeID);
            else return new ODataItemProvider(ApiSettings).GetPriceType(PriceTypeID);
        }
        public List<PriceType> GetPriceTypes()
        {
            if(ApiSettings.IsEnterprise) return new SqlItemProvider(ApiSettings).GetPriceTypes();
            else return new ODataItemProvider(ApiSettings).GetPriceTypes();
        }

        public Warehouse GetWarehouse(int WarehouseID)
        {
            if(ApiSettings.IsEnterprise) return new SqlItemProvider(ApiSettings).GetWarehouse(WarehouseID);
            else return new ODataItemProvider(ApiSettings).GetWarehouse(WarehouseID);
        }
        public List<Warehouse> GetWarehouses()
        {
            if(ApiSettings.IsEnterprise) return new SqlItemProvider(ApiSettings).GetWarehouses();
            else return new ODataItemProvider(ApiSettings).GetWarehouses();
        }

        public Language GetLanguage(int LanguageID)
        {
            if(ApiSettings.IsEnterprise) return new SqlItemProvider(ApiSettings).GetLanguage(LanguageID);
            else throw new Exception("You must have Enterprise-level access to use the GetLanguage() method.");
        }
        public List<Language> GetLanguages()
        {
            if(ApiSettings.IsEnterprise) return new SqlItemProvider(ApiSettings).GetLanguages();
            else throw new Exception("You must have Enterprise-level access to use the GetLanguages() method.");
        }
    }
    #endregion

    #region OData
    public class ODataItemProvider : BaseODataProvider, IItemService
    {
        public ODataItemProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public PriceType GetPriceType(int PriceTypeID)
        {
            var result = new PriceType();

            var response = GetContext().PriceTypes
                .Where(c => c.PriceTypeID == PriceTypeID)
                .FirstOrDefault();
            if(response == null) return null;

            result.PriceTypeID = response.PriceTypeID;
            result.PriceTypeDescription = response.PriceTypeDescription;

            return result;
        }
        public List<PriceType> GetPriceTypes()
        {
            var result = new List<PriceType>();


            // Get all records recursively
            var maxRecordsPerCall = GlobalSettings.OData.MaxRecordsPerCall;
            int lastResultCount = maxRecordsPerCall;
            int completedCallCount = 0;

            while (lastResultCount == maxRecordsPerCall)
            {
                var response = GetContext().PriceTypes
                    .Skip(completedCallCount * maxRecordsPerCall)
                    .Take(maxRecordsPerCall)
                    .Select(c => new PriceType()
                    {
                        PriceTypeID = c.PriceTypeID,
                        PriceTypeDescription = c.PriceTypeDescription
                    }).ToList();

                response.ForEach(c => result.Add(c));

                completedCallCount++;
                lastResultCount = response.Count;
            }


            return result;
        }

        public Warehouse GetWarehouse(int WarehouseID)
        {
            var result = new Warehouse();

            var response = GetContext().Warehouses
                .Where(c => c.WarehouseID == WarehouseID)
                .FirstOrDefault();
            if(response == null) return null;

            result.WarehouseID = response.WarehouseID;
            result.WarehouseDescription = response.WarehouseDescription;

            return result;
        }
        public List<Warehouse> GetWarehouses()
        {
            var result = new List<Warehouse>();


            // Get all records recursively
            var maxRecordsPerCall = GlobalSettings.OData.MaxRecordsPerCall;
            int lastResultCount = maxRecordsPerCall;
            int completedCallCount = 0;

            while (lastResultCount == maxRecordsPerCall)
            {
                var response = GetContext().Warehouses
                    .Skip(completedCallCount * maxRecordsPerCall)
                    .Take(maxRecordsPerCall)
                    .Select(c => new Warehouse()
                    {
                        WarehouseID = c.WarehouseID,
                        WarehouseDescription = c.WarehouseDescription
                    }).ToList();

                response.ForEach(c => result.Add(c));

                completedCallCount++;
                lastResultCount = response.Count;
            }


            return result;
        }

        public Language GetLanguage(int LanguageID)
        {
            if(ApiSettings.IsEnterprise) return new SqlItemProvider(ApiSettings).GetLanguage(LanguageID);
            else throw new Exception("You must have Enterprise-level access to use the GetLanguage() method.");
        }
        public List<Language> GetLanguages()
        {
            if(ApiSettings.IsEnterprise) return new SqlItemProvider(ApiSettings).GetLanguages();
            else throw new Exception("You must have Enterprise-level access to use the GetLanguages() method.");
        }
    }
    #endregion

    #region Sql
    public class SqlItemProvider : BaseSqlProvider, IItemService
    {
        public SqlItemProvider(IApiSettings ApiSettings = null) : base(ApiSettings) {}

        public PriceType GetPriceType(int PriceTypeID)
        {
            var result = new PriceType();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        *
                    FROM PriceTypes
                    WHERE PriceTypeID = {0}
                ", PriceTypeID))
            {
                if(!reader.Read()) return null;

                result.PriceTypeID = reader.GetInt32("PriceTypeID");
                result.PriceTypeDescription = reader.GetString("PriceTypeDescription");
            }

            return result;
        }
        public List<PriceType> GetPriceTypes()
        {
            var result = new List<PriceType>();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        *
                    FROM PriceTypes
                "))
            {
                while (reader.Read())
                {
                    result.Add(new PriceType()
                    {
                        PriceTypeID = reader.GetInt32("PriceTypeID"),
                        PriceTypeDescription = reader.GetString("PriceTypeDescription")
                    });
                }
            }

            return result;
        }

        public Warehouse GetWarehouse(int PriceTypeID)
        {
            var result = new Warehouse();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        *
                    FROM Warehouses
                    WHERE WarehouseID = {0}
                ", PriceTypeID))
            {
                if(!reader.Read()) return null;

                result.WarehouseID = reader.GetInt32("WarehouseID");
                result.WarehouseDescription = reader.GetString("WarehouseDescription");
            }

            return result;
        }
        public List<Warehouse> GetWarehouses()
        {
            var result = new List<Warehouse>();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        *
                    FROM Warehouses
                "))
            {
                while (reader.Read())
                {
                    result.Add(new Warehouse()
                    {
                        WarehouseID = reader.GetInt32("WarehouseID"),
                        WarehouseDescription = reader.GetString("WarehouseDescription")
                    });
                }
            }

            return result;
        }

        public Language GetLanguage(int PriceTypeID)
        {
            var result = new Language();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        *
                    FROM Languages
                    WHERE LanguageID = {0}
                ", PriceTypeID))
            {
                if(!reader.Read()) return null;

                result.LanguageID = reader.GetInt32("LanguageID");
                result.LanguageDescription = reader.GetString("LanguageDescription");
            }

            return result;
        }
        public List<Language> GetLanguages()
        {
            var result = new List<Language>();

            using (var reader = GetContext().GetReader(@"
                    SELECT
                        *
                    FROM Languages
                "))
            {
                while (reader.Read())
                {
                    result.Add(new Language()
                    {
                        LanguageID = reader.GetInt32("LanguageID"),
                        LanguageDescription = reader.GetString("LanguageDescription")
                    });
                }
            }

            return result;
        }
    }
    #endregion
}
