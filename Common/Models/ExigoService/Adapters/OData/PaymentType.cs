namespace Common.Api.ExigoOData
{
    public partial class PaymentType
    {
        public static explicit operator ExigoService.PaymentType(PaymentType type)
        {
            var model = new ExigoService.PaymentType();
            if (type == null) return model;

            model.PaymentTypeID          = type.PaymentTypeID;
            model.PaymentTypeDescription = type.PaymentTypeDescription;

            return model;
        }
    }
}