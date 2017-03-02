using Common;
using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static IEnumerable<Language> GetLanguages()
        {
            // Get a list of the available languages
            var availableLanguageIDs = GlobalSettings.Globalization.AvailableLanguages.Select(c => c.LanguageID).ToList();
            if (availableLanguageIDs.Count == 0) yield break;

            var context = Exigo.OData();
            var results = context.Languages
                .Where(availableLanguageIDs.ToOrExpression<Common.Api.ExigoOData.Language, int>("LanguageID"))
                .ToList();


            // Populate the available language or the one we got back from the server.
            foreach (var result in results)
            {
                yield return (Language)result;
            }
        }
        public static Language GetLanguage(int languageID)
        {
            // Try to return the first available language we have 
            var result = GlobalSettings.Globalization.AvailableLanguages.Where(c => c.LanguageID == languageID).FirstOrDefault();
            if (result != null)
            {
                return (Language)result;
            }

            // If we couldn't find it, get the languages and return it
            return GetLanguages().Where(c => c.LanguageID == languageID).FirstOrDefault();
        }
    }
}