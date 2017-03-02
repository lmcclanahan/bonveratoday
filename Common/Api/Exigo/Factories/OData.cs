using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Services.Client;
using Common;

namespace ExigoService
{
    public static partial class Exigo
    {
        private static T GetODataContext<T>(int sandboxID = 0) where T : DataServiceContext
        {
            return CreateODataContext<T>(sandboxID);
        }
        public static T CreateODataContext<T>(int sandboxID) where T : DataServiceContext
        {
            // Determine some helpful variables
            var type = typeof(T);
            var typeName = type.Name;
            var schemaName = string.Empty;
            var url = string.Empty;

            // Determine which URL we should use
            switch (typeName)
            {
                case "ExigoContext": schemaName = "model"; break;
                case "ExigoReportingContext": schemaName = "reporting"; break;
                default: schemaName = typeName; break;
            }
            url = GetODataUrl(schemaName);

            // Create the context
            T context = (T)Activator.CreateInstance(typeof(T), new Uri(url));
            context.IgnoreMissingProperties = true;
            context.IgnoreResourceNotFoundException = true;
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(GlobalSettings.Exigo.Api.LoginName + ":" + GlobalSettings.Exigo.Api.Password));
            context.SendingRequest += (object s, SendingRequestEventArgs e) =>
                    e.RequestHeaders.Add("Authorization", "Basic " + credentials);

            return context;
        }

        private static string GetODataUrl()
        {
            return GetODataUrl(null, GlobalSettings.Exigo.Api.SandboxID);
        }
        private static string GetODataUrl(int sandboxID)
        {
            return GetODataUrl(null, sandboxID);
        }
        private static string GetODataUrl(string schema)
        {
            return GetODataUrl(schema, GlobalSettings.Exigo.Api.SandboxID);
        }
        private static string GetODataUrl(string schema, int sandboxID)
        {
            var urlFormat = "http://{0}.exigo.com/4.0/{1}/{2}";

            var cname = "api";
            if (sandboxID > 0)
            {
                cname = "sandboxapi" + sandboxID;
            }

            var dbschema = string.Empty;
            if (!string.IsNullOrEmpty(schema))
            {
                var standardSchemas = new List<string> { "model", "reporting" };
                if (standardSchemas.Contains(schema)) dbschema = schema;
                else dbschema = "db/" + schema;
            }

            return string.Format(urlFormat, cname, GlobalSettings.Exigo.Api.CompanyKey, dbschema);
        }
    }
}