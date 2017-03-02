using Backoffice.Models.CommissionPayout;
using Common.Api.ExigoWebService;
using ExigoService;
using System.Web.Mvc;

namespace Backoffice.Services
{
    public class Commissions
    {
        public static CommissionPayout GetDirectDeposit()
        {
            var account = new CommissionPayout();

            try
            {
                var result = Exigo.WebService().GetAccountDirectDeposit(new GetAccountDirectDepositRequest
                {
                    CustomerID = Identity.Current.CustomerID
                });

                if (result.Result.Status == ResultStatus.Success)
                {
                    account.AccountNumber = result.BankAccountNumberDisplay;
                    account.NameOnAccount = result.NameOnAccount;
                    account.BankName = result.BankName;
                    account.RoutingNumber = result.BankRoutingNumber;
                    // 79630 09/01/2016 PM Added DepositAccountType
                    account.DepositAccountTypeForm = result.DepositAccountType;

                    //account.BillingAddress.Address1 = result.BankAddress;
                    //account.BillingAddress.City = result.BankCity;
                    //account.BillingAddress.State = result.BankState;
                    //account.BillingAddress.Country = result.BankCountry;
                    //account.BillingAddress.Zip = result.BankZip;
                }
            }
            catch
            {

            }

            return account;
        }

        public static bool SetDirectDeposit(CommissionPayout account)
        {
            try
            {
                var result = Exigo.WebService().SetAccountDirectDeposit(new SetAccountDirectDepositRequest
                {
                    CustomerID = Identity.Current.CustomerID,
                    NameOnAccount = account.NameOnAccount,
                    BankName = account.BankName,
                    BankRoutingNumber = account.RoutingNumber,
                    BankAccountNumber = account.AccountNumber,
                    // 79630 09/01/2016 PM DepositAccountType was set to Checking by default, added option to set to EIN
                    DepositAccountType = account.DepositAccountTypeForm,

                    //BankAddress = account.BillingAddress.Address1,
                    //BankCity = account.BillingAddress.City,
                    //BankState = account.BillingAddress.State,
                    //BankCountry = account.BillingAddress.Country,
                    //BankZip = account.BillingAddress.Zip
                });
            }
            catch
            {
                return false;
            }

            //Adds payable type and bank information to user defined fields as done in client's past
            try
            {
                var updateCustomerRequest = Exigo.WebService().UpdateCustomer(new UpdateCustomerRequest
                {
                    CustomerID = Identity.Current.CustomerID,
                    PayableType = PayableType.DirectDeposit,
                    Field1 = account.BankName,
                    Field2 = account.RoutingNumber,
                    Field3 = account.AccountNumber,
                    Field4 = account.NameOnAccount,
                });

            }
            catch
            {

                return false;
            }

            return true;
        }
    }
}