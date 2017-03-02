
namespace Common.Api.ExigoOData
{
    public partial class Language
    {
        public static explicit operator ExigoService.Language(Language language)
        {
            var model = new ExigoService.Language();
            if (language == null) return model;

            model.LanguageID          = language.LanguageID;
            model.LanguageDescription = model.GetLanguageDescription();
            model.CultureCode         = language.CultureCode;

            return model;
        }
    }
}