using System.Collections.Generic;
using System.Linq;
using ExigoService;
using Common.Api.ExigoWebService;

namespace Common
{
    public static class GlobalSettings
    {
        // Global Settings
        public static bool HideForLive = false;//Needs to be removed as this client is now launched 
        //L.G 8-2-2016 #78978: Added new variable to switch between Production and UAT Urls
        public static bool UseUATUrls = true;

        public static string BaseReplicatedeUrl = (UseUATUrls) ? "http://bonverauat.azurewebsites.net" : "http://bonvera.com"; //http://bonverauat.azurewebsites.net
        public static string BaseBackofficeUrl = (UseUATUrls) ? "http://bonverabackofficeuat.azurewebsites.net" : "http://backoffice.bonvera.com"; //http://bonverabackofficeuat.azurewebsites.net

        public static string SqlConnectionString
        {
            get
            {
                if (!Exigo.Api.UseSandboxGlobally)
                {
                    // Live SQL
                    return "Data Source=allbrands.bi.exigo.com;Initial Catalog=allbrandsreporting;Persist Security Info=True;User ID=ExigoWeb;Password=mgt5NnpT&&96;Pooling=False";
                }
                else
                {
                    // Sandbox SQL
                    return "Data Source=sandbox.bi.exigo.com;Initial Catalog=allbrandsreportingsandbox2;Persist Security Info=True;User ID=ExigoWeb;Password=mgt5NnpT&&96;Pooling=False";
                }
            }
        }


        /// <summary>
        /// Exigo-specific API credentials and configurations
        /// </summary>
        public static class Exigo
        {
            /// <summary>
            /// Web service, OData and SQL API credentials and configurations
            /// </summary>
            public static class Api
            {
                public const string LoginName = "API_Bonvera";
                public const string Password = "API_7&3$@!gHQ";
                //Errors out without exception and won't authenticate when CompanyKey is incorrect
                public const string CompanyKey = "allbrands";

                public static bool UseSandboxGlobally = true;
                public static int SandboxID { get { return (UseSandboxGlobally) ? 2 : 0; } }

                /// <summary>
                /// Replicated SQL connection strings and configurations
                /// </summary>
                public static class Sql
                {
                    public static class ConnectionStrings
                    {
                        public static string SqlReporting = SqlConnectionString;
                    }
                }
            }

            /// <summary>
            /// Payment API credentials
            /// </summary>
            public static class PaymentApi
            {
                public const string LoginName = "allbrands_fYC4v9Vf1";
                public const string Password = "KKxi1VcfR5A6PU27J28X43qZ";
            }
        }

        /// <summary>
        /// Default backoffice settings
        /// </summary>
        public static class Backoffices
        {
            public static int SessionTimeout = 15; // In minutes

            /// <summary>
            /// Silent login URL's and configurations
            /// </summary>
            public static class SilentLogins
            {
                public static string DistributorBackofficeUrl = Company.BaseBackofficeUrl + "/silentlogin/?token={0}";
                public static string RetailCustomerBackofficeUrl = ReplicatedSites.FormattedBaseUrl.FormatWith(ReplicatedSites.DefaultWebAlias) + "/account/silentlogin/?token={0}";
                public static string RetailCustomerBackofficeUrl_Formatted = ReplicatedSites.FormattedBaseUrl + "/account/silentlogin/?token={1}";
            }

            public static string EnrollNowLandingPageUrl = Company.BaseBackofficeUrl + "/app/enrollmentredirect";

            /// <summary>
            /// Waiting room configurations
            /// </summary>
            public static class WaitingRooms
            {
                /// <summary>
                /// The number of days a customer can be placed in a waiting room after their initial enrollment.
                /// </summary>
                public static int GracePeriod = 30; // In days
            }

            public static string MonthlySubscriptionItemCode = "BOMF";
            public static string MonthlySubscriptionCookieName = GlobalSettings.Exigo.Api.CompanyKey + "_MonthlySubscription";
        }

        /// <summary>
        /// Default replicated site settings
        /// </summary>
        public static class ReplicatedSites
        {
            public static int SessionTimeout = 30; // In minutes
            public static string DefaultWebAlias = "corporphan";
            public static int OrphanCustomerID = 5;
            public static int DefaultAccountID = 5; // Company Account
            public static int IdentityRefreshInterval = 15; // In minutes
            public static string FormattedBaseUrl = Company.BaseReplicatedeUrl + "/{0}";
            public static string GetFormattedUrl(string webAlias)
            {
                return FormattedBaseUrl.FormatWith(webAlias);
            }
            public static string EnrollmentUrl = FormattedBaseUrl + "/enrollment";
            public static string GetEnrollmentUrl(object webAlias)
            {
                return string.Format(EnrollmentUrl, webAlias);
            }
        }

        /// <summary>
        /// Market configurations used for orders, autoOrders, products and more
        /// </summary>
        public static class Markets
        {
            //JS, 09/11/2015
            //Removed the Comma because Commas will break Cookie Names in Safari
            public static string MarketCookieName = Company.Name.Replace(",", "") + "SelectedMarket";

            public static List<Market> AvailableMarkets
            {
                get
                {
                    return new List<Market>
                    {
                        new UnitedStatesMarket()
                        //new CanadaMarket()
                    };
                }
            }
        }

        /// <summary>
        /// Language and culture code configurations
        /// </summary>
        public static class Globalization
        {
            //Removed the Comma because Commas will break Cookie Names in Safari
            public static string CountryCookieName = Company.Name.Replace(",", "") + "SelectedCountry";
            public static string SiteCultureCookieName = "SiteCulture";
            public static string LanguageCookieName = Company.Name.Replace(",", "") + "SelectedLanguage";
            public static List<Language> AvailableLanguages
            {
                get
                {
                    return new List<Language>
                    {
                        new Language(Languages.English, "en-US")
                        //, new Language(Languages.French, "fr-FR")
                    };
                }
            }

            /// <summary>
            /// Set this to true or false depending on if we are dealing with the soft launch site or the eventual live version - Mike M.
            /// </summary>
            public static bool HideForLive = GlobalSettings.HideForLive;
        }

        /// <summary>
        /// Language and culture code configurations
        /// </summary>
        public static class AutoOrders
        {
            public static int MaxAutoOrderCount = 2;

            public static List<int> AvailableFrequencyTypeIDs
            {
                get
                {
                    return new List<int>
                    {
                        FrequencyTypes.Monthly,
                        FrequencyTypes.Quarterly,
                        FrequencyTypes.Yearly
                    };
                }
            }
            public static List<FrequencyType> AvailableFrequencyTypes = AvailableFrequencyTypeIDs.Select(c => ExigoService.Exigo.GetFrequencyType(c)).ToList();
        }

        /// <summary>
        /// Customer avatar configurations
        /// </summary>
        public static class Avatars
        {
            public static string DefaultAvatarAsBase64 = "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/2wBDAQkJCQwLDBgNDRgyIRwhMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjL/wAARCAEsASwDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD0WiiigAooooAKKKKACiiigBO1LRRQAneloooAKKKKAEpaKKACiiigApO1LRQAUUUUAFFFFABRRRQAUUUUAJ3paKKACiiigAoopO1AC0UUUAFFFFABRSd6WgAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAopKWgBOKWiigAooooAKKTvS0AJS0UUAFJS0UAFFFFABRRRQAUnelooAKKKKACiiigApO9LRQAnaloooAKKKKACiiigBKKWigAopO9LQAUlLRQAUUUUAFJS0UAJS0UUAFFFFACUtFFABSUtFABSUtJQAtFFFABRRRQAUUUUAFFFFABRRRQAUUUnegBaKKKACinRxvK4SNSzHsK17bQJGwbiTYP7q8mgDGpyo7/AHULfQV1cOl2cH3YQx9W5q4FVRhQB9KAOMFpdHpbSn/gBprW06fehkH1Q121FAHCkYNFdrJbwyjEkSN9VrPn0O1lyY90Te3IoA5qir11pVzaguV3oP4lqjQAUUneloAKKKKACiiigAooooAKKKKACiiigAooooAKKTvS0AFFFFABRRRQAUnalooAKKKKACiiigAq9YaZJetuOUiHVvX6U/S9NN3J5knEKn/vr2rp1VUUKoAUcADtQBFbWkNomyFMep7mp6KKACiiigAooooAKKKKACsq/wBHiuQXhAjl/Rq1aKAOIlieCQxyKVYdQaZXXX9hHfRYPEgHyt6Vyk0TwStHIuGWgBlFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFACd6WiigAooooAKKKKACk5paKACrFlaNeXKxDherH0FV66jRrUW9mJCP3kvzH6dqAL8USQxLGgwqjAFPoooAKKKKACikpaACiiigAooooAKKKKACszV7D7TD5qD96g7fxD0rTooA4Wir+r2gtbzKDCSfMPaqFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQBPZQ/abyOLszc/SuyAwMDpXO+H4g11JIf4VwPxro6ACiiigAooooAKKO1HagAooooAO9HakpaACiiigAooooAzdat/OsGcfejO4f1rl67eRBJE6HoykGuJZdrFT1BwaAEooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAoopO1AHQ+Hl/cTN6uBW1WP4e/49JR/00/pWxQAUUUUAFFFJ3oAWiiigAooooAKKKKACiiigAooooAK4y+XbfTr/tmuzrjtROdRuP8AfNAFaiiigAooooAKKKKACiiigAooooAKKKKACkpaKACiiigAooooAKKKKAN3w6/+vj+jVu1y2izeVqKqTxICtdTQAUUUUAFFJS0AFFFFABRRRQAUUUUAFFFFABRRRQAVxVw/mXMr/wB5yf1rrL6b7PZTSdwpx9a46gAooooAKKKKACiiigAooooAKKKKAE70tFFABRRRQAUUUnegBaKKKACik7UtACo5jkV14ZTkV2dvMLi3SVejDNcXW1oV7tc2rn5W5T60AdBRRRQAUUdqKACikpaACiiigAooooAO1FFFABRRUU0yQQtK5wqjJoAyNfucLHbA9fmb+lYNS3E73M7zP95j+VRUAFJ2paTvQAtFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFKrFWDA4I5BpKKAOq0zUFvIcMcTL94evvWhXEQzPBKssbbWXvXT6fqUV6m0kLMOq+v0oA0KKKKACiiigAooooAKKKKACiimswRSzEBR1JoAUkAZNczq+ofapPJiP7lD1/vGpNU1bz8wW5Ij6M396sigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACk70tFABRRRQAUUUnegBaKKKAClVmRgykhh0Iq5Z6XcXnzAbI/7zD+Vb1ppVta4bb5kn95qAINMvLudQs0DFe0vStaiigAopKWgAooooAKKKKAGSu0cZZELkdFB61y+o311cSFJlaJR/BjFdXUUsEc6bZUDr7igDiqK3bvQOr2r/APAG/wAaxZI3hcpIhVh2IoAZRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUVJBBJcSrHEu5jQAxEeRwiKWY8ACugsNFSLElzh36hewq3YadHZJnhpT95sfyq9QAgAAwKWiigAooooAKKSloAKKKKACiiigAooo7UAFV7mzhu02ypn0PcVYooA5K+02WybP34j0cD+dUq7hlV1KsAVPUGuc1PSTbZmgBMXcf3aAMqiiigAooooAKKKKACiiigAooooAKKKTtQAtFFFABRSUtABRRRgmgB8UTzSrHGu5m4ArqtPsUsYcDmRvvNUOk6eLWLzZB++f8A8dHpWnQAUUUd6ACjtRRQAUlFLQAUUUUAFFFFABRRRQAUUUUAFFFFABSEAjB6UtFAHNarpn2ZjNEP3THkf3ayq7h0WRCrgMp4INcnqNi1lcYHMbcqf6UAU6KKKACiiigAooooAKKKKACiik7UALRRRQAlFLRQAVr6LY+dL9okHyIflHqazLeB7idIU+8xxXY28K28CRIAFUYoAlooooAKKKKACiiigAooooAKO1FFABRRRQAUUUUAFFFFABRRRQAUUUUAFVr21S8tmibg9VPoas0UAcPJG0UjRuMMpwRTa3des+l0g9n/AKGsKgAooooAKKKKACiiigAooooAKKKKACiiljRpJFRclmOBQBu6BagK10w5Pyp/WtyoreFbeBIl6KMVLQAUUUUAFFFHagApKWigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKAGSxrNE0bjKsMGuMuImt7h4m6qcV21c/4gttskdwo4b5W+tAGLRRRQAUUUUAFFFFACd6WiigAopKWgArT0O382+8wj5Yxu/HtWZXSaDDssmkPWRv0FAGtRR2ooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAqpqMH2mxlTGTjK/UVbooA4Wip72HyL2aPsG4+lQUAFFFFABRRRQAUUUUAFFFFABXZWUfk2UMfogz9a5GBPMuI0/vOBXa0ALRRRQAUlLRQAUUUUAFFFFABRRSUAHeloooAKKKKACiiigAo7UUUAFFFFABR2oooAKKKKACiiigDmtfjC3qPj76frWVXQeIY8wQyf3WxXP0AFFFFABRRRQAUUUUAFFJ2ooAuaWu/U4B/tZ/KuvrldF51SP2B/lXVUAFHaiigA70UlLQAUUd6O9ABRRRQAlLRR3oAKKKKACiiigAooooAKKKKACiiigAoo70UAFFFFABRRR3oAzdcXdpjn+6wNcvXW6tzpc/0H865KgAooooAKKTvS0Af//Z";
        }

        /// <summary>
        /// Error logging configuration
        /// </summary>
        public static class ErrorLogging
        {
            public static bool ErrorLoggingEnabled = false;
            public static string[] EmailRecipients = new string[] { "@exigo.com" };
        }

        /// <summary>
        /// Email configurations
        /// </summary>
        public static class Emails
        {
            public static string NoReplyEmail = "Support@bonvera.com";
            public static string SupportEmail = "support@bonvera.com";
            public static string VerifyEmailUrl = Company.BaseBackofficeUrl + "/verifyemail";

            // NEED NEW CREDS FROM CLIENT IF THEY ARE TO SEND ANY EMAILS FROM THE WEB
            public static class SMTPConfigurations
            {
                public static SMTPConfiguration Default = new SMTPConfiguration
                {
                    Server = "smtp.sendgrid.net",
                    Port = 25,
                    Username = "azure_db83b156fc75b5954285689218476967@azure.com",
                    Password = "Varada2016",
                    EnableSSL = false
                };
            }
        }

        /// <summary>
        /// Company information
        /// </summary>
        public static class Company
        {
            public static string Name = "Bonvera";
            public static Address Address = new Address()
            {
                Address1 = "1815 E. Central",
                Address2 = "",
                City = "Wichita",
                State = "KS",
                Zip = "67214",
                Country = "US"
            };
            public static string Phone = "(555)555-5555";
            public static string Email = "support@bonvera.com";
            public static string Facebook = "https://www.facebook.com/BonveraUS";
            public static string Twitter = "https://twitter.com/bonvera_US";
            public static string YouTube = "http://youtube.com/";
            public static string Blog = "http://blogger.net/blog/";
            public static string Pinterest = "http://www.pinterest.com";
            public static string GooglePlus = "https://www.google.com/";
            public static string Instagram = "https://www.instagram.com/bonvera_US/";
            public static string DefaultCompanyMessage = "This is our company statement";

            public static string BaseReplicatedeUrl = GlobalSettings.BaseReplicatedeUrl;
            public static string BaseBackofficeUrl = GlobalSettings.BaseBackofficeUrl;
        }

        /// <summary>
        /// EncryptionKeys used for silent logins and other AES encryptions
        /// </summary>
        public static class EncryptionKeys
        {
            public static string General = "UB7920GAW61UPX4FI1GS60Ob"; // 24 characters 

            public static class SilentLogins
            {
                public static string Key = GlobalSettings.Exigo.Api.CompanyKey + "silentlogin";
                public static string IV = "6!sn3$3FERc76UPl"; // Must be 16 characters long
            }
        }

        public static class RegularExpressions
        {
            public const string EmailAddresses = "^[0-9a-zA-Z]+([0-9a-zA-Z]*[-._+])*[0-9a-zA-Z]+@[0-9a-zA-Z]+([-.][0-9a-zA-Z]+)*([0-9a-zA-Z]*[.])[a-zA-Z]{2,6}$";
            public const string EmailAddressesOld = "[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?";
            public const string LoginName = "^[a-zA-Z0-9]+$";
            public const string Password = "^.{1,50}$";
            public const string TaxID = @"^\d{9}$";
        }

        /// <summary>
        /// Debug Module Configuration
        /// </summary>
        public static class Debug
        {
            //JS, 09/15/2015
            //Added Cookie Name for Debug Parameters
            public static string DebugCookieName = GlobalSettings.Exigo.Api.CompanyKey + "_DebugMode";
        }

    }

    public enum MarketName
    {
        UnitedStates,
        Canada
    }
    public enum AvatarType
    {
        Tiny,
        Small,
        Default,
        Large
    }
    public enum SocialNetworks
    {
        Facebook = 1,
        GooglePlus = 2,
        Twitter = 3,
        Blog = 4,
        LinkedIn = 5,
        MySpace = 6,
        YouTube = 7,
        Pinterest = 8,
        Instagram = 9
    }

    public enum TreeTypes
    {
        Binary = 1,
        Unilevel = 2
    }
    public enum PartnerStoreTypes
    {
        AllBrandsForYou,
        FullerBrush,
        Eskimo,
        Isabelles,
        Bullionworks,
        Quilbed
    }

    public static class CustomerStatusTypes
    {
        public const int Active = 1;
        public const int Terminated = 2;
        public const int Inactive = 3;
        public static int[] ActiveStatusTypes = new int[] { CustomerStatusTypes.Active };
    }

    public static class NewsDepartments
    {
        public const int Backoffice = 7;
        public const int Replicated = 8;
    }

    /// List For Enrollment Replinish Packs
    /// </summary>
    public static class ReplinishPacks
    {

        public static List<string> PackItemCodes
        {
            get
            {
                return new List<string>
                    {
                        "PHYZIXFAMILYASSORT",
                        "PHYZIXMVPASSORT",
                        "PHYZIXSINGLETROP"
                    };
            }
        }

    }
}
