using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Exigo.Api
{
    public class SqlHelper
    {
        public SqlHelper(string ConnectionString)
        {
            _connectionString = ConnectionString;
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }
        private string _connectionString;

        public SqlDataReader GetReader(string cmdText, params object[] paramList)
        {
            /*using (var connection = new SqlConnection(ConnectionString))
            using (var command    = GetParamCommand(cmdText, paramList))
            {
                connection.Open();
                command.Connection   = connection;
                SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }*/

            var connection = new SqlConnection(ConnectionString);
            var command    = GetParamCommand(cmdText, paramList);
            
            connection.Open();
            command.Connection   = connection;
            SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
            
        }
        public DataSet GetSet(string cmdText, params object[] paramList)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command    = GetParamCommand(cmdText, paramList))
            {                
                connection.Open();
                command.Connection         = connection;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataSet dataset            = new DataSet();
                dataAdapter.Fill(dataset);
                return dataset;
            }
        }
        public DataTable GetTable(string cmdText, params object[] paramList)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command    = GetParamCommand(cmdText, paramList))
            {
                connection.Open();
                command.Connection         = connection;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataTable datatable        = new DataTable();
                dataAdapter.Fill(datatable);
                return datatable;
            }
        }
        public DataRow GetRow(string cmdText, params object[] paramList)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command    = GetParamCommand(cmdText, paramList))
            {
                connection.Open();
                command.Connection         = connection;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataTable datatable        = new DataTable();
                dataAdapter.Fill(datatable);
                if (datatable.Rows.Count > 0) return datatable.Rows[0];
                else return null;
            }
        }
        public object GetField(string cmdText, params object[] paramList)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command    = GetParamCommand(cmdText, paramList))
            {
                connection.Open();
                command.Connection = connection;
                object o = command.ExecuteScalar();
                return o;                
            }
        }
        public int Execute(string cmdText, params object[] paramList)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command    = GetParamCommand(cmdText, paramList))
            {
                connection.Open();
                command.Connection     = connection;
                command.CommandTimeout = 1200; // 20 minutes
                return command.ExecuteNonQuery();
            }
        }

        private SqlCommand GetParamCommand(string cmdText, object[] paramList)
        {
            SqlCommand cmd = new SqlCommand();


            if (paramList.Length > 0)
            {
                object[] a = new object[paramList.Length];
                for (int i = 0; i < paramList.Length; i++)
                {
                    a[i] = "@Param" + i.ToString();
                }
                cmd.CommandText = string.Format(cmdText, a);
                for (int i = 0; i < paramList.Length; i++)
                {
                    if (paramList[i].GetType().Equals(typeof(String)))
                    {
                        cmd.Parameters.Add("@Param" + i.ToString(), SqlDbType.NVarChar, 8000).Value = paramList[i];
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Param" + i.ToString(), paramList[i]);
                    }
                }
            }
            else
            {
                cmd.CommandText = cmdText;
            }

            return cmd;
        }
    }
}