using Common;
using Common.Api.ExigoOData.ResourceManager;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static resourcescontext ODataResources(int sandboxID = 0)
        {
            return GetODataContext<resourcescontext>(((sandboxID > 0) ? sandboxID : GlobalSettings.Exigo.Api.SandboxID));
        }
    }
}
