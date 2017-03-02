using Common.Api.ExigoOData.Calendars;
using Common;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static calendarcontext ODataCalendars(int sandboxID = 0)
        {
            return GetODataContext<calendarcontext>(((sandboxID > 0) ? sandboxID : GlobalSettings.Exigo.Api.SandboxID));
        }
    }
}