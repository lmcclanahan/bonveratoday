using Common.Api.ExigoOData;
using Common;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static ExigoContext OData(int sandboxID = 0)
        {
            return GetODataContext<ExigoContext>(((sandboxID > 0) ? sandboxID : GlobalSettings.Exigo.Api.SandboxID));
        }
    }
}