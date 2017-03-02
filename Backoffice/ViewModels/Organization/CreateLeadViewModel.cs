﻿using ExigoService;

namespace Backoffice.ViewModels
{
    public class CreateLeadViewModel
    {
        public CreateLeadViewModel()
        {
            this.Lead = new Customer();
            this.Lead.MainAddress = new Address();
        }

        public Customer Lead { get; set; }
       
    }
}