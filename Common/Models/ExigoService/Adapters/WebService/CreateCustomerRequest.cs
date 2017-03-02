using ExigoService;
using System;

namespace Common.Api.ExigoWebService
{
    public partial class CreateCustomerRequest
    {
        public CreateCustomerRequest() { }
        public CreateCustomerRequest(Customer customer)
        {
            CustomerType        = customer.CustomerTypeID;
            CustomerStatus      = customer.CustomerStatusID;
            DefaultWarehouseID  = customer.DefaultWarehouseID;
            LanguageID          = customer.LanguageID;
            EntryDate           = customer.CreatedDate;
                        
            // DateTime.MinValue is used as the flag to show they have opted not to save their bday info
            if (customer.BirthDate != DateTime.MinValue)
            {
                BirthDate = customer.BirthDate;
            }

            FirstName           = customer.FirstName;
            MiddleName          = customer.MiddleName;
            LastName            = customer.LastName;
            Email               = customer.Email;
            Phone               = customer.PrimaryPhone;
            Phone2              = customer.SecondaryPhone;
            MobilePhone         = customer.MobilePhone;
            Fax                 = customer.Fax;

            if (customer.MainAddress != null)
            {
                MainAddress1 = customer.MainAddress.Address1;
                MainAddress2 = customer.MainAddress.Address2;
                MainCity     = customer.MainAddress.City;
                MainState    = customer.MainAddress.State;
                MainZip      = customer.MainAddress.Zip;
                MainCountry  = customer.MainAddress.Country;
            }

            if (customer.MailingAddress != null)
            {
                MailAddress1 = customer.MailingAddress.Address1;
                MailAddress2 = customer.MailingAddress.Address2;
                MailCity     = customer.MailingAddress.City;
                MailState    = customer.MailingAddress.State;
                MailZip      = customer.MailingAddress.Zip;
                MailCountry  = customer.MailingAddress.Country;
            }

            if (customer.OtherAddress != null)
            {
                OtherAddress1 = customer.OtherAddress.Address1;
                OtherAddress2 = customer.OtherAddress.Address2;
                OtherCity     = customer.OtherAddress.City;
                OtherState    = customer.OtherAddress.State;
                OtherZip      = customer.OtherAddress.Zip;
                OtherCountry  = customer.OtherAddress.Country;
            }

            TaxID         = customer.TaxID;
            PayableToName = customer.PayableToName;
            PayableType   = Exigo.GetPayableType(customer.PayableTypeID);

            LoginName = customer.LoginName;
            LoginPassword = customer.Password;

            if (customer.EnrollerID != null)
            {
                InsertEnrollerTree = true;
                EnrollerID = (int)customer.EnrollerID;
            }

            if (customer.SponsorID != null)
            {
                InsertUnilevelTree = true;
                SponsorID = (int)customer.SponsorID;
            }

            if(customer.IsOptedIn)
            {
                SubscribeToBroadcasts  = customer.IsOptedIn;
                SubscribeFromIPAddress = GlobalUtilities.GetClientIP();
            }

            Field1  = customer.Field1;
            Field2  = customer.Field2;
            Field3  = customer.Field3;
            Field4  = customer.Field4;
            Field5  = customer.Field5;
            Field6  = customer.Field6;
            Field7  = customer.Field7;
            Field8  = customer.Field8;
            Field9  = customer.Field9;
            Field10 = customer.Field10;
            Field11 = customer.Field11;
            Field12 = customer.Field12;
            Field13 = customer.Field13;
            Field14 = customer.Field14;
            Field15 = customer.Field15;

            Date1 = customer.Date1;
            Date2 = customer.Date2;
            Date3 = customer.Date3;
            Date4 = customer.Date4;
            Date5 = customer.Date5;
        }
    }
}