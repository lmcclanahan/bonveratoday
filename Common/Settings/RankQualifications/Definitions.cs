using System.Collections.Generic;

namespace ExigoService
{
    public static partial class Exigo
    {
        private static readonly IEnumerable<IRankRequirementDefinition> RankQualificationDefinitions = new List<IRankRequirementDefinition>
        { 
            Boolean("Customer Type", 
                Expression: @"^MUST BE A VALID CUSTOMER TYPE",
                Description: "You must be an Independent Associate.",
                Qualified: "",
                NotQualified: "You are not an Independent Associate."
            ),

            Boolean("Status",  
                Expression: @"^MUST HAVE A VALID STATUS",
                Description: "Your membership status must be active.",
                Qualified: "",
                NotQualified: "Your membership status is not active."
            ),

            Boolean("Active", 
                Expression: @"^ACTIVE$", 
                Description: "You must be considered Active.",
                Qualified: "",
                NotQualified: "You must be Active in order to qualify for the next rank."
            ),

            Boolean("Qualified", 
                Expression: @"^MUST BE QUALIFIED$",
                Description: "You must be qualified to receive commissions.",
                Qualified: "",
                NotQualified: "You must be qualified for commissions in order to advance to the next rank."
            ),


            Boolean("Rank Override",       
                Expression: @"^MUST HAVE A RANK OVERRIDE OF",
                Description: "You must have a rank override of {{RequiredValue}} or Greater.",
                Qualified: "",
                NotQualified: "You do not have the rank override."
            ),
            Decimal("ACTIVE", 
                Expression: @"^\d+ ACTIVE$",
                //80080 - L.A. - 9/2/2016 - Changed text per customers request.
                Description: "You must be active with 100 PBV.",
                Qualified: "",
                NotQualified: "You must be active with 100 PBV to advance."
            ),

            Decimal("Personally Enrolled Associates", 
                Expression: @"^\d+ PERSONALLY ENROLLED/SPONSORED ACTIVE ASSOCIATES IN DIFFERENT LEGS",
                Description: "You need at least 1 Personally Enrolled/Sponsored Active Associate in {{RequiredValueAsDecimal:N0}} different legs.",
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more Personally Enrolled Active Associates</strong> to advance."
            ),
            // OLD
            //Decimal("Personally Enrolled Associates", 
            //    Expression: @"^\d+ PERSONALLY ENROLLED ACTIVE ASSOCIATES",
            //    Description: "You need at least 1 Personally Enrolled Active Associate in {{RequiredValueAsDecimal:N0}} different legs.",
            //    Qualified: "",
            //    NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more Personally Enrolled Active Associates</strong> to advance."
            //),
            
            Decimal("3leg tgbv", 
                Expression: @"^[0-9]+(,[0-9]+)* LEG WITH 2000 TGBV",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} legs with 2000 TGBV.",
                Qualified: "",
                NotQualified: "You need {{RequiredValueAsDecimal:N0}} legs with <strong>2000 more TGBV</strong> to advance."
            ),
             Decimal("3leg tgbv", 
                Expression: @"^[0-9]+(,[0-9]+)* LEG WITH 1000 TGBV",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} legs with 1000 TGBV.",
                Qualified: "",
                NotQualified: "You need {{RequiredValueAsDecimal:N0}} legs with <strong>1000 more TGBV</strong> to advance."
            ),
             Decimal("unilevel active professional legs", 
                Expression: @"^[0-9]+(,[0-9]+)* UNILEVEL ACTIVE PROFESSIONAL LEGS",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} Unilevel Active Professional Legs.",
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more Unilevel Active Professional Legs</strong> to advance."
            ),
             Decimal("unilevel active executive legs", 
                Expression: @"^[0-9]+(,[0-9]+)* UNILEVEL ACTIVE EXECUTIVE LEGS",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} Unilevel Active Executive Legs.",
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more Unilevel Active Executive Legs</strong> to advance."
            ),
             Decimal("unilevel active senior legs", 
                Expression: @"^[0-9]+(,[0-9]+)* UNILEVEL ACTIVE SENIOR EXECUTIVE LEGS",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} Unilevel Active Senior Executive Legs.",
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more Unilevel Active Senior Executive Legs</strong> to advance."
            ),
             Decimal("unilevel active ambassador legs", 
                Expression: @"^[0-9]+(,[0-9]+)* UNILEVEL ACTIVE AMBASSADOR LEGS",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} Unilevel Active Ambassador Legs.",
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more Unilevel Active Ambassador Legs</strong> to advance."
            ),
             Decimal("TGBV", 
                Expression: @"^\d+ TOTAL GROUP BONUS VALUE",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} Total Group Bonus Value.",
                Qualified: "",
                NotQualified: "You need <strong>{{FormattedAmountNeededToQualify:N0}} more Total Group Bonus Value</strong> to advance."
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 900 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 900 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 900 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 1050 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 1050 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 1050 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 3150 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 3150 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 3150 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 3000 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 3000 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 3000 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 30000 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 30000 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 30000 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 6000 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 6000 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 6000 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 6300 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 6300 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 6300 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 14000 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 14000 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 14000 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 15000 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 15000 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 15000 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 28000 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 28000 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 28000 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 40000 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 40000 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 40000 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 100000 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 100000 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 100000 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 200000 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 200000 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 200000 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            ),
            Decimal("TGBV capped at per leg", 
                Expression: @"^MUST HAVE [0-9]+(,[0-9]+)* TGBV CAPPED AT 400000 PER LEG",
                Description: "You need at least {{RequiredValueAsDecimal:N0}} TGBV capped at 400000 per leg.",
                Qualified: "",
                NotQualified: "Your current TGBV capped at 400000 per leg is insufficient. You need <strong>{{FormattedAmountNeededToQualify}} more</strong> to advance"
            )


        };
    }
}