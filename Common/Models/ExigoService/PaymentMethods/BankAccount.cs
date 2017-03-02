using Common.Api.ExigoWebService;
using System.ComponentModel.DataAnnotations;

namespace ExigoService
{
    public class BankAccount : IBankAccount
    {
        public BankAccount()
        {
            this.Type = BankAccountType.New;
            this.BillingAddress = new Address();
            this.AutoOrderIDs = new int[0];
        }
        public BankAccount(BankAccountType type)
        {
            Type = type;
            BillingAddress = new Address();
        }

        [Required]
        public BankAccountType Type { get; set; }

        [Required(ErrorMessage = "Please Enter The Name on the Bank Account"), Display(Name = "Name on Account")]
        public string NameOnAccount { get; set; }

        [Required(ErrorMessage="Bank Name is Required"), Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "A Valid Account Number is Required"), Display(Name = "Account Number"), RegularExpression(@"^\d{1,15}$", ErrorMessage = "Please Enter a Valid Account Number")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage="A Valid Routing Number is Required"), Display(Name = "Routing Number"), RegularExpression(@"^\d{9}$", ErrorMessage="Please Enter a Valid 9 DIgit Routing Number")]
        public string RoutingNumber { get; set; }

        [Required, DataType("Address")]
        public Address BillingAddress { get; set; }

        public int[] AutoOrderIDs { get; set; }


        public bool IsComplete
        {
            get
            {
                if (string.IsNullOrEmpty(NameOnAccount)) return false;
                if (string.IsNullOrEmpty(BankName)) return false;
                if (string.IsNullOrEmpty(AccountNumber)) return false;
                if (string.IsNullOrEmpty(RoutingNumber)) return false;
                if (!BillingAddress.IsComplete) return false;

                return true;
            }
        }
        public bool IsValid
        {
            get
            {
                if (!IsComplete) return false;

                return true;
            }
        }
        public bool IsUsedInAutoOrders
        {
            get { return this.AutoOrderIDs.Length > 0; }
        }

        public AutoOrderPaymentType AutoOrderPaymentType
        {
            get
            {
                switch (this.Type)
                {
                    case BankAccountType.Primary:
                    default: return Common.Api.ExigoWebService.AutoOrderPaymentType.CheckingAccount;
                }
            }
        }
    }
}