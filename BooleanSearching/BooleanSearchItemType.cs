using System.ComponentModel;

namespace BooleanSearching
{
    public enum BooleanSearchItemType
    {
        [Description("Represents an AND boolean operator for BooleanSearchItem's type")]
        BooleanOperatorAND = 0,
        [Description("Represents an OR boolean operator for BooleanSearchItem's type")]
        BooleanOperatorOR = 1,
        [Description("Represents an NOT boolean operator for BooleanSearchItem's type")]
        BooleanOperatorNOT = 2,
        [Description("Represents an BooleanSearchItem that needs to be evaluated")]
        Evaluate = 3,
        [Description("Search string between quotes. Is for exact, case sensitive searching")]
        ExactSearchString = 4,
        [Description("Normal search string, case insensitive searching")]
        SearchString = 5
    }
}