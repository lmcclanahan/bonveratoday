using Common.Api.ExigoAdminWebService;
using Common;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static ExigoApiAdmin AdminWebService(int sandboxID = 0)
        {
            return GetAdminWebServiceContext(((sandboxID > 0) ? sandboxID : GlobalSettings.Exigo.Api.SandboxID));
        }
    }
}