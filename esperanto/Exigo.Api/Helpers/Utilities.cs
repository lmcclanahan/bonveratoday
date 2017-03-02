using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exigo.Api.Helpers
{
    public static class GlobalUtilities
    {
        /// <summary>
        /// Sets the value of the string to be the first non-nullable parameter found for the strings provided.
        /// </summary>
        /// <param name="strings"></param>
        /// <returns>The first non-null, non-empty string found.</returns>
        public static string Coalesce(params string[] strings)
        {
            return strings.Where(s => !string.IsNullOrEmpty(s)).FirstOrDefault();
        }
    }
}
