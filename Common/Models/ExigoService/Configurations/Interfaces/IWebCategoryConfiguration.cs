using Common;
using Common.Api.ExigoWebService;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExigoService
{
    public interface IWebCategoryConfiguration
    {
        List<ItemCategory> Categories { get; }

        ItemCategory EnrollmentKits { get; }
        ItemCategory Accessories { get; }
        
        ItemCategory Health { get; }
        ItemCategory Drinks { get; }
        ItemCategory Sprays { get; }
    }                                       
}