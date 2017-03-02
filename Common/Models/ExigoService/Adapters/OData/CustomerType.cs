
namespace Common.Api.ExigoOData
{
    public partial class CustomerType
    {
        public static explicit operator ExigoService.CustomerType(CustomerType type)
        {
            var model = new ExigoService.CustomerType();
            if (type == null) return model;

            model.CustomerTypeID          = type.CustomerTypeID;
            model.CustomerTypeDescription = type.CustomerTypeDescription;

            return model;
        }
    }
}