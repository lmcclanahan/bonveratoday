using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;

namespace ExigoService
{
    public static class ODataExtensions
    {
        public static List<T> SelectAll<T, TSource>(this TSource context, Func<TSource, IQueryable<T>> action)
            where T : class
            where TSource : DataServiceContext
        {
            var results = new List<T>();

            const int recordCount = 50;
            var lastResultsCount = recordCount;
            var callsMade = 0;

            while (lastResultsCount == recordCount)
            {
                var response = action(context)
                    .Skip(callsMade * recordCount)
                    .Take(recordCount)
                    .ToList();

                lastResultsCount = response.Count;

                results.AddRange(response);

                callsMade++;
            }

            return results;
        }
    }
}
