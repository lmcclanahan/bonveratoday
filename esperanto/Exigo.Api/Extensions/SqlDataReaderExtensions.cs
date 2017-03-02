using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api.Extensions
{
    public static class SqlDataReaderExtensions
    {
        public static string GetString(this SqlDataReader reader, string ColumnName)
        {
            return reader.GetString(reader.GetOrdinal(ColumnName));
        }
        public static int GetInt32(this SqlDataReader reader, string ColumnName)
        {
            return reader.GetInt32(reader.GetOrdinal(ColumnName));
        }
        public static decimal GetDecimal(this SqlDataReader reader, string ColumnName)
        {
            return reader.GetDecimal(reader.GetOrdinal(ColumnName));
        }
        public static DateTime GetDateTime(this SqlDataReader reader, string ColumnName)
        {
            return reader.GetDateTime(reader.GetOrdinal(ColumnName));
        }
        public static bool GetBoolean(this SqlDataReader reader, string ColumnName)
        {
            return reader.GetBoolean(reader.GetOrdinal(ColumnName));
        }
        public static object GetObject(this SqlDataReader reader, string ColumnName)
        {
            return reader.GetValue(reader.GetOrdinal(ColumnName));
        }
        public static Address GetAddress(this SqlDataReader reader, string Address1ColumnName, string Address2ColumnName, string CityColumnName, string StateColumnName, string ZipColumnName, string CountryColumnName)
        {
            var address      = new Address();

            address.Address1 = reader.GetString(Address1ColumnName);
            address.Address2 = reader.GetString(Address2ColumnName);
            address.City     = reader.GetString(CityColumnName);
            address.State    = reader.GetString(StateColumnName);
            address.Zip      = reader.GetString(ZipColumnName);
            address.Country  = reader.GetString(CountryColumnName);

            return address;

        }
    }
}
