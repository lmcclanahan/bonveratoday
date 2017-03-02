
namespace Common.Api.ExigoOData
{
    public partial class CustomerStatus
    {
        public static explicit operator ExigoService.CustomerStatus(CustomerStatus status)
        {
            var model = new ExigoService.CustomerStatus();
            if (status == null) return model;

            model.CustomerStatusID          = status.CustomerStatusID;
            model.CustomerStatusDescription = status.CustomerStatusDescription;

            return model;
        }
    }
}