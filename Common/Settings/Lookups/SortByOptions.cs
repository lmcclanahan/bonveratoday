using System.ComponentModel;
namespace Common
{
    public enum SortByOptions
    {

        /// <summary>
        ///	Alphabetical Ascending 0
        /// </summary>
        [Description("Alphabetical Ascending")]
        AtoZ_Ascending = 0,

        /// <summary>
        ///	Alphabetical Descending 1
        /// </summary>
        [Description("Alphabetical Descending")]
        AtoZ_Descending = 1,

        /// <summary>
        ///	BV Ascending 2
        /// </summary>
        [Description("BV Ascending")]
        BV_Ascending = 2,

        /// <summary>
        ///	BV Descending 3
        /// </summary>
        [Description("BV Descending")]
        BV_Descending = 3,

        /// <summary>
        ///	Price Ascending 4
        /// </summary>
        [Description("Price Ascending")]
        Price_Ascending = 4,

        /// <summary>
        ///	Price Descending 5
        /// </summary>
        [Description("Price Descending")]
        Price_Descending = 5

    }
}
